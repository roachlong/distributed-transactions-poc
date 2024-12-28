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
        // hard-coded configuration which should be part of the app environment configuration
        const string topic = "rebalancing-requests";
        const int numTasks = 1;
        const int maxRetries = 5;

        // adding a listener for graceful shutdown with Ctrl+C
        CancellationTokenSource cts = new();
        Console.CancelKeyPress += (_, e) => {
            e.Cancel = true;
            cts.Cancel();
        };

        // creating tasks to run indefinitely as separate process threads
        var tasks = new List<Task>();
        for (int i = 0; i < numTasks; i++) {
            tasks.Add(ReceiveRequests(
                topic, maxRetries, cts.Token
            ));
        }

        // wait for all tasks to complete
        await Task.WhenAll(tasks);
        cts.Dispose();
    }

    private static async Task ReceiveRequests(
        string topic, int maxRetries, CancellationToken token
    ) {
        Console.WriteLine($"Launching consumer for {topic}...");
        await Task.Run(async () => {
            // track number of consecutive exceptions
            int attempts = 0;
            // our kafka consumer configuration
            var config = RebalanceRequestConfig.GetConfig();

            // run until process cancelled or consectutive exceptions exceeds max retires
            while (!token.IsCancellationRequested && attempts < maxRetries) {
                // establish a consumer object and wait 1 second before subscribing
                using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
                await Task.Delay(1000, token);
                
                // each consumer uses a shared group id and can be scaled across any number of nodes
                consumer.Subscribe(topic);
                try {
                    // read from the consumer until the process is cancelled or an exception is thrown
                    while (!token.IsCancellationRequested) {
                        // block forever until the process is cancelled
                        var msg = consumer.Consume(token);

                        // if no message consumed then continue
                        if (!token.IsCancellationRequested && msg != null && msg.Message.Value != null) {
                            var json = JsonObject.Parse(msg.Message.Value);

                            // if a valid message was received then process it
                            if (json != null && json["after"] != null) {
                                // we need to add serializable options for our json messages where
                                // ISO date formats can appear with or without the nano seconds
                                var serializeOptions = new JsonSerializerOptions {
                                    WriteIndented = true
                                };
                                serializeOptions.Converters.Add(new MultiFormatDateConverter {
                                    DateTimeFormats = [
                                        "yyyy-MM-ddTHH:mm:ssZ",
                                        "yyyy-MM-ddTHH:mm:ss.fffffZ",
                                        "yyyy-MM-ddTHH:mm:ss.ffffffZ"
                                    ]
                                });
                                
                                // we expect a rebalancing message in the "post" commit part of the CDC
                                RebalancingRequest? request = JsonSerializer.Deserialize<RebalancingRequest>(
                                    json["after"], serializeOptions
                                );
                                
                                // if the rebalancing message is valid then decompose the request
                                if (request != null) {
                                    // get a new connection which should come from an external pool
                                    using var context = new OrdersDbContext();
                                    // and split the rebalancing request by unique security
                                    context.InsertRebalancingSecurities(request, maxRetries);
                                }
                                // otherwise report an error, which should also be published to an error queue
                                else {
                                    Console.WriteLine($"ERROR: invalid request received: {msg.Message.Value}");
                                }
                            }
                            
                            // commit the message after the rebalancing request has been evaluated
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
        Console.WriteLine($"Completed consumer for {topic}!!");
    }
}