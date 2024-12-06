using System.Text.Json;
using System.Text.Json.Nodes;
using Confluent.Kafka;
using DistributedTransactions.Data;
using DistributedTransactions.Domain.Orders;
using DistributedTransactions.Data.Util;

namespace DistributedTransactions.RebalanceRequest;

class RebalanceRequestProgram
{
    static void Main(string[] args) {
        MainAsync().Wait();
    }

    private static async Task MainAsync() {
        const string topic = "rebalancing-requests";
        const int numPartitions = 10;
        const int maxRetries = 5;

        CancellationTokenSource cts = new();
        Console.CancelKeyPress += (_, e) => {
            e.Cancel = true;
            cts.Cancel();
        };

        var tasks = new List<Task>();
        for (int i = 0; i < numPartitions; i++) {
            tasks.Add(ReceiveRequests(
                topic, i, maxRetries, cts.Token
            ));
        }
        await Task.WhenAll(tasks);
        cts.Dispose();
    }

    private static async Task ReceiveRequests(
        string topic, int partition, int maxRetries, CancellationToken token
    ) {
        Console.WriteLine($"Launching consumer {partition} for {topic}...");
        await Task.Run(async () => {
            var config = RebalanceRequestConfig.GetConfig();
            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();

            if (partition <= 1) {
                consumer.Subscribe(topic);
            }
            else {
                consumer.Assign(new TopicPartition(topic, partition));
            }
            await Task.Delay(1000, token);
            try {
                while (!token.IsCancellationRequested) {
                    var msg = consumer.Consume(token);
                    if (!token.IsCancellationRequested && msg.Message.Value != null) {
                        var json = JsonObject.Parse(msg.Message.Value);
                        if (json != null && json["after"] != null) {
                            var serializeOptions = new JsonSerializerOptions {
                                WriteIndented = true
                            };
                            serializeOptions.Converters.Add(new MultiFormatDateConverter {
                                DateTimeFormats = [
                                    "yyyy-MM-ddTHH:mm:ssZ",
                                    "yyyy-MM-ddTHH:mm:ss.ffffffZ"
                                ]
                            });
                            RebalancingRequest? request = JsonSerializer.Deserialize<RebalancingRequest>(
                                json["after"], serializeOptions
                            );
                            if (request != null) {
                                using var context = new OrdersDbContext();
                                context.InsertRebalancingSecurities(request, maxRetries);
                            }
                        }
                        consumer.Commit(msg);
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
                Console.WriteLine($"Received {e.GetType()}: {e.Message}");
            }
            finally {
                await Task.Delay(1000, token);
                consumer.Unsubscribe();
                consumer.Close();
            }
        });
        Console.WriteLine($"Completed consumer {partition} for {topic}!!");
    }
}