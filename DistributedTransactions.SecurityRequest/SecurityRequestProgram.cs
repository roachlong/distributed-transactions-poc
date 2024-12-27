using System.Text.Json;
using System.Text.Json.Nodes;
using Confluent.Kafka;
using DistributedTransactions.Data;
using DistributedTransactions.Domain.Orders;

namespace DistributedTransactions.SecurityRequest;

class SecurityRequestProgram
{
    static void Main(string[] args) {
        MainAsync().Wait();
    }
    
    private static async Task MainAsync() {
        // hard-coded configuration which should be part of the app environment configuration
        const string topic = "security-requests";
        const int numTasks = 5;
        const int batchSize = 128;
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
            tasks.Add(GenerateOrders(
                topic, batchSize, maxRetries, cts.Token
            ));
        }

        // wait for all tasks to complete
        await Task.WhenAll(tasks);
        cts.Dispose();
    }

    private static async Task GenerateOrders(
        string topic, int batchSize,
        int maxRetries, CancellationToken token
    ) {
        Console.WriteLine($"Launching consumer for {topic}...");
        await Task.Run(async () => {
            // object to generate random values for trade attributes
            var random = new Random();
            // track number of consecutive exceptions
            int attempts = 0;
            // our kafka consumer configuration
            var config = SecurityRequestConfig.GetConfig();

            // run until process cancelled or consectutive exceptions exceeds max retires
            while (!token.IsCancellationRequested && attempts < maxRetries) {
                // establish a consumer object and wait 10 seconds before subscribing
                using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
                await Task.Delay(10000, token);

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
                                // we expect a security message in the "post" commit part of the CDC
                                RebalancingSecurity? security = JsonSerializer.Deserialize<RebalancingSecurity>(json["after"]);
                                
                                // if the security message is valid then create the orders
                                if (security != null)
                                {
                                    // get a new connection which should come from an external pool
                                    using var context = new OrdersDbContext();

                                    // get a list of customer orders based on drift calculations
                                    var count = GenerateCustomerOrders(
                                        security, random, context, batchSize, maxRetries
                                    );

                                    // if any positions require rebalancing
                                    if (count > 0) {
                                        // then aggregate them into a single block order
                                        context.CreateBlockOrder(security, maxRetries);
                                    }
                                }
                                // otherwise report an error, which should also be published to an error queue
                                else {
                                    Console.WriteLine($"ERROR: invalid security received: {msg.Message.Value}");
                                }
                            }
                            
                            // commit the message after the security positions have been evaluated
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

    private static int GenerateCustomerOrders(
        RebalancingSecurity security, Random random,
        OrdersDbContext context, int batchSize, int maxRetries
    ) {
#pragma warning disable CS8605 // Unboxing a possibly null value.
        Array values = Enum.GetValues(typeof(OrderDestination));
        var value = random.Next(values.Length);
        var destination = (OrderDestination)values.GetValue(value);

        values = Enum.GetValues(typeof(OrderType));
        value = random.Next(values.Length);
        var type = (OrderType)values.GetValue(value);

        values = Enum.GetValues(typeof(OrderRestriction));
        value = random.Next(values.Length);
        var restriction = (OrderRestriction)values.GetValue(value);
#pragma warning restore CS8605 // Unboxing a possibly null value.

        var orders = new List<CustomerOrder>();
        int count = 0;
        var positions = context.GetOpeningPositions(security, maxRetries);
        foreach (var position in positions) {
            var required = (int)(position.PortfolioValue * position.Allocation / position.Open);
            var need = required - position.Quantity;
            if (need == 0) {
                continue;
            }
            var direction = need < 0 ? OrderDirection.Sell : OrderDirection.Buy;

            var order = new CustomerOrder {
                RequestNumber = security.RequestNumber,
                AccountNum = position.AccountNum,
                PositionId = position.PositionId,
                AssetClass = security.AssetClass,
                Symbol = security.Symbol,
                Date = security.Date,
                Direction = direction,
                Destination = destination,
                Type = type,
                Restriction = restriction,
                Quantity = Math.Abs(need)
            };

            orders.Add(order);
            if (orders.Count >= batchSize) {
                count += context.InsertCustomerOrders(orders, maxRetries);
                orders = [];
            }
        }

        if (orders.Count > 0) {
            count += context.InsertCustomerOrders(orders, maxRetries);
        }
        return count;
    }
}
