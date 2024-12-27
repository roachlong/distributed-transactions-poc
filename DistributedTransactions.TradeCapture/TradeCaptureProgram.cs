using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Confluent.Kafka;
using DistributedTransactions.Data;
using DistributedTransactions.Domain.Allocations;

namespace DistributedTransactions.TradeCapture;

class TradeCaptureProgram
{
    static void Main(string[] args) {
        MainAsync().Wait();
    }

    private static async Task MainAsync() {
        // hard-coded configuration which should be part of the app environment configuration
        const string topic = "trade-executions";
        const int numPartitions = 10;
        const int batchSize = 128;
        const int batchWindow = 1000;
        const int maxRetries = 5;
        const bool useMultiValueInsert = true;

        // adding a listener for graceful shutdown with Ctrl+C
        CancellationTokenSource cts = new();
        Console.CancelKeyPress += (_, e) => {
            e.Cancel = true;
            cts.Cancel();
        };

        // creating tasks to run indefinitely as separate process threads
        var tasks = new List<Task>();
        for (int i = 0; i < numPartitions; i++) {
            tasks.Add(CaptureTrades(
                topic, i, batchSize, batchWindow, maxRetries, useMultiValueInsert, cts.Token
            ));
        }

        // wait for all tasks to complete
        await Task.WhenAll(tasks);
        cts.Dispose();
    }

    private static async Task CaptureTrades(
        string topic, int partition, int batchSize, int batchWindow,
        int maxRetries, bool useMultiValueInsert, CancellationToken token
    ) {
        Console.WriteLine($"Launching consumer {partition} for {topic}...");
        await Task.Run(async () => {
            // internal buffer for "batch" of trade messages
            var trades = new List<Trade>();
            // track number of consecutive exceptions
            int attempts = 0;
            // our kafka consumer configuration
            var config = TradeCaptureConfig.GetConfig(partition);
            // start the watch for a time limit on flushing the batch
            Stopwatch window = Stopwatch.StartNew();

            // run until process cancelled or consectutive exceptions exceeds max retires
            while (!token.IsCancellationRequested && attempts < maxRetries) {
                // establish a consumer object and wait 10 seconds before subscribing
                using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
                await Task.Delay(10000, token);
                
                // each consumer is assigned to a specific partition for ordering of trade messages
                consumer.Assign(new TopicPartition(topic, partition));
                try {
                    // read from the consumer until the process is cancelled or an exception is thrown
                    while (!token.IsCancellationRequested) {
                        // don't block forever, we want to process messages within our batch window  
                        var msg = consumer.Consume(batchWindow);

                        // if no message consumed then continue
                        if (!token.IsCancellationRequested && msg != null && msg.Message.Value != null) {
                            var json = JsonObject.Parse(msg.Message.Value);

                            // if a valid message was received then process it
                            if (json != null && json["after"] != null) {
                                // we expect a trade message in the "post" commit part of the CDC
                                Trade? trade = JsonSerializer.Deserialize<Trade>(json["after"]);

                                // if the trade message is valid add it to our internal buffer
                                if (trade != null) {
                                    trades.Add(trade);
                                }
                                // otherwise report an error, which should also be published to an error queue
                                else {
                                    Console.WriteLine($"ERROR: invalid trade record received: {msg.Message.Value}");
                                }

                                // flush the trades to the database if batch size or time limit exceeded
                                if (trades.Count >= batchSize || window.ElapsedMilliseconds >= batchWindow) {
                                    // get a new connection which should come from an external pool
                                    using var context = new AllocationsDbContext();
                                    if (useMultiValueInsert) {
                                        // we should always use multi-value inserts for performance
                                        context.InsertTradesRawSql(trades, maxRetries);
                                    }
                                    else {
                                        // but also linq won't work because there's no way to handle
                                        // conflicts when a duplicate message is received
                                        context.AddRange(trades);
                                        context.SaveChanges();
                                    }
                            
                                    // commit the last message we received before flushing the internal buffer
                                    consumer.StoreOffset(msg);

                                    // database transaction succeeded, reset state for next batch
                                    attempts = 0;
                                    trades = [];
                                    window = Stopwatch.StartNew();
                                }
                            }
                            // otherwise report an error, which should also be published to an error queue
                            else {
                                Console.WriteLine($"ERROR: invalid trade message received: {msg.Message.Value}");
                            }
                        }

                        // if we've stopped consuming due to the process being cancelled
                        // and/or we've also waited past our batch window time limit
                        // make sure we flush our internal buffer of acknowledged trade messages
                        if (trades.Count > 0 && (
                            token.IsCancellationRequested || window.ElapsedMilliseconds >= batchWindow
                        )) {
                            // get a new connection which should come from an external pool
                            using var context = new AllocationsDbContext();
                            if (useMultiValueInsert) {
                                // we should always use multi-value inserts for performance
                                context.InsertTradesRawSql(trades, maxRetries);
                            }
                            else {
                                // but also linq won't work because there's no way to handle
                                // conflicts when a duplicate message is received
                                context.AddRange(trades);
                                context.SaveChanges();
                            }
                            
                            // commit the last message we received before flushing the internal buffer
                            consumer.StoreOffset(msg);

                            // database transaction succeeded, reset state for next batch
                            attempts = 0;
                            trades = [];
                            window = Stopwatch.StartNew();
                        }
                    }
                }
                catch (OperationCanceledException) {
                    Console.WriteLine("Cancellation request received");
                }
                catch (ConsumeException e) {
                    Console.WriteLine($"ERROR consuming message: {e.Error.Reason}");
                }
                catch (Exception e) {
                    if (e != null) {
                        Console.WriteLine($"Received {e.GetType()}: {e.Message}");
                    }
                    else {
                        Console.WriteLine($"Caught an unknown exception");
                    }
                }
                finally {
                    consumer.Unsubscribe();
                    consumer.Close();

                    // in case of consecutive exceptions we'll progressively cool off before the next attempt
                    attempts++;
                    await Task.Delay(1000 * attempts, token);
                }
            }
        });
        Console.WriteLine($"Completed consumer {partition} for {topic}!!");
    }
}