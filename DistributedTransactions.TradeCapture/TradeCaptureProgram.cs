using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Confluent.Kafka;
using DistributedTransactions.Data;
using DistributedTransactions.Domain.Allocations;

namespace DistributedTransactions.Allocations;

class TradeCaptureProgram
{
    static void Main(string[] args) {
        MainAsync().Wait();
    }

    private static async Task MainAsync() {
        const string topic = "trade-executions";
        const int numPartitions = 10;
        const int batchSize = 128;
        const int batchWindow = 200;
        const int maxRetries = 5;
        const bool useMultiValueInsert = true;

        CancellationTokenSource cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => {
            e.Cancel = true;
            cts.Cancel();
        };

        var tasks = new List<Task>();
        for (int i = 0; i < numPartitions; i++) {
            tasks.Add(CaptureTrades(topic, i, batchSize, batchWindow, maxRetries, useMultiValueInsert));
        }
        await Task.WhenAll(tasks);
    }

    private static async Task CaptureTrades(
        string topic, int partition, int batch_size, int batch_window, int maxRetries, bool useMultiValueInsert
    ) {
        Console.WriteLine($"Launching consumer {partition} for {topic}...");
        await Task.Run(async () => {
            var config = TradeCaptureConfig.GetConfig();
            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            var trades = new List<Trade>();

            if (partition <= 1) {
                consumer.Subscribe(topic);
            }
            else {
                consumer.Assign(new TopicPartition(topic, partition));
            }
            await Task.Delay(1000);
            try {
                Stopwatch window = Stopwatch.StartNew();
                while (true) {
                    var msg = consumer.Consume();
                    var json = JsonObject.Parse(msg.Message.Value);
                    if (json != null) {
                        Trade? trade = JsonSerializer.Deserialize<Trade>(json["after"]);
                        if (trade != null) {
                            trades.Add(trade);
                        }
                        if (trades.Count >= batch_size || window.ElapsedMilliseconds >= batch_window) {
                            using var context = new AllocationsDbContext();
                            if (useMultiValueInsert) {
                                context.InsertTradesRawSql(trades, maxRetries);
                            }
                            else {
                                context.AddRange(trades);
                                context.SaveChanges();
                            }
                            trades = [];
                            window = Stopwatch.StartNew();
                        }
                    }
                    consumer.Commit(msg);
                }
            }
            catch (OperationCanceledException) {
                // Ctrl-C was pressed.
                return;
            }
            catch (ConsumeException e) {
                Console.WriteLine($"ERROR consuming message: {e.Error.Reason}");
            }
            finally {
                consumer.Close();
            }
        });
        Console.WriteLine($"Completed consumer {partition} for {topic}!!");
    }
}