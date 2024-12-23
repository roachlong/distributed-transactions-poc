﻿using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Confluent.Kafka;
using DistributedTransactions.Data;
using DistributedTransactions.Domain.Allocations;
using DistributedTransactions.Domain.Orders;
using Microsoft.IdentityModel.Tokens;

namespace DistributedTransactions.TradeFills;

class TradeFillsProgram
{
    static void Main(string[] args) {
        MainAsync().Wait();
    }

    private static async Task MainAsync() {
        // hard-coded configuration which should be part of the app environment configuration
        const string topic = "trade-allocations";
        const int numPartitions = 10;
        const int batchSize = 128;
        const int batchWindow = 200;
        const int maxRetries = 5;

        // adding a listener for graceful shutdown with Ctrl+C
        CancellationTokenSource cts = new();
        Console.CancelKeyPress += (_, e) => {
            e.Cancel = true;
            cts.Cancel();
        };

        // creating tasks to run indefinitely as separate process threads
        var tasks = new List<Task>();
        for (int i = 0; i < numPartitions; i++) {
            tasks.Add(CaptureTradeFills(
                topic, i, batchSize, batchWindow, maxRetries, cts.Token
            ));
        }

        // wait for all tasks to complete
        await Task.WhenAll(tasks);
        cts.Dispose();
    }

    private static async Task CaptureTradeFills(
        string topic, int partition, int batchSize, int batchWindow,
        int maxRetries, CancellationToken token
    ) {
        Console.WriteLine($"Launching consumer {partition} for {topic}...");
        await Task.Run(async () => {
            // internal buffer for "batch" of trade fill messages
            var fills = new List<TradeFill>();
            var backlog = new Dictionary<String, Queue<TradeFill>>();
            // track number of consecutive exceptions
            int attempts = 0;
            // our kafka consumer configuration
            var config = TradeFillsConfig.GetConfig(partition);
            // start the watch for a time limit on flushing the batch
            Stopwatch window = Stopwatch.StartNew();

            // run until process cancelled or consecutive exceptions exceeds max retires
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
                                    var fill = new TradeFill {
                                        Code = trade.BlockOrderCode,
                                        Date = trade.Date,
                                        FilledQuantity = trade.Quantity,
                                        Price = trade.Price,
                                        CancelledQuantity = trade.CancelledQuantity,
                                        NewDestination = trade.NewDestination == null ? null :
                                            (OrderDestination) (int) trade.NewDestination,
                                        NewType = trade.NewType == null ? null :
                                            (OrderType) (int) trade.NewType,
                                        NewRestriction = trade.NewRestriction == null ? null :
                                            (OrderRestriction) (int) trade.NewRestriction,
                                        NewQuantity = trade.NewQuantity
                                    };
                                    if (fills.Any(f => f.Code.Equals(fill.Code))) {
                                        if (!backlog.ContainsKey(fill.Code)) {
                                            backlog.Add(fill.Code, new Queue<TradeFill>());
                                        }
                                        backlog[fill.Code].Enqueue(fill);
                                    } else {
                                        fills.Add(fill);
                                    }
                                }
                                // otherwise report an error, which should also be published to an error queue
                                else {
                                    Console.WriteLine($"ERROR: invalid trade record received: {msg.Message.Value}");
                                }

                                // flush the trades to the database if batch size or time limit exceeded
                                if (fills.Count >= batchSize || window.ElapsedMilliseconds >= batchWindow) {
                                    // get a new connection which should come from an external pool
                                    using var context = new OrdersDbContext();
                                    context.UpdateBlockOrderFills(fills, maxRetries);

                                    // database transaction succeeded, reset state for next batch
                                    attempts = 0;
                                    fills = [];
                                    foreach (KeyValuePair<String, Queue<TradeFill>> queue in backlog) {
                                        if (queue.Value.IsNullOrEmpty()) {
                                            backlog.Remove(queue.Key);
                                        }
                                        else {
                                            fills.Add(queue.Value.Dequeue());
                                        }
                                    }
                                    window = Stopwatch.StartNew();
                                }
                            }
                            // otherwise report an error, which should also be published to an error queue
                            else {
                                Console.WriteLine($"ERROR: invalid trade message received: {msg.Message.Value}");
                            }
                            
                            // commit the message after the trade has been added to our internal buffer
                            consumer.Commit(msg);
                        }

                        // if we've stopped consuming due to the process being cancelled
                        // and/or we've also waited past our batch window time limit
                        // make sure we flush our internal buffer of acknowledged trade messages
                        if (fills.Count > 0 && (
                            token.IsCancellationRequested || window.ElapsedMilliseconds >= batchWindow
                        )) {
                            // get a new connection which should come from an external pool
                            using var context = new OrdersDbContext();
                            context.UpdateBlockOrderFills(fills, maxRetries);

                            // database transaction succeeded, reset state for next batch
                            attempts = 0;
                            fills = [];
                            foreach (KeyValuePair<String, Queue<TradeFill>> queue in backlog) {
                                if (queue.Value.IsNullOrEmpty()) {
                                    backlog.Remove(queue.Key);
                                }
                                else {
                                    fills.Add(queue.Value.Dequeue());
                                }
                            }
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