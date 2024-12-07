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
        const string topic = "security-requests";
        const int numTasks = 20;
        const int batchSize = 128;
        const int maxRetries = 5;

        CancellationTokenSource cts = new();
        Console.CancelKeyPress += (_, e) => {
            e.Cancel = true;
            cts.Cancel();
        };

        var tasks = new List<Task>();
        for (int i = 0; i < numTasks; i++) {
            tasks.Add(GenerateOrders(
                topic, batchSize, maxRetries, cts.Token
            ));
        }
        await Task.WhenAll(tasks);
        cts.Dispose();
    }

    private static async Task GenerateOrders(
        string topic, int batchSize,
        int maxRetries, CancellationToken token
    ) {
        Console.WriteLine($"Launching consumer for {topic}...");
        await Task.Run(async () => {
            var random = new Random();
            var config = SecurityRequestConfig.GetConfig();
            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            await Task.Delay(1000, token);
            consumer.Subscribe(topic);
            try {
                while (!token.IsCancellationRequested) {
                    var msg = consumer.Consume(token);
                    if (!token.IsCancellationRequested && msg.Message.Value != null) {
                        var json = JsonObject.Parse(msg.Message.Value);
                        if (json != null && json["after"] != null) {
                            RebalancingSecurity? security = JsonSerializer.Deserialize<RebalancingSecurity>(json["after"]);
                            if (security != null)
                            {
                                using var context = new OrdersDbContext();
                                var orders = GenerateCustomerOrders(
                                    security, random, context, batchSize, maxRetries
                                );
                                if (orders.Count > 0) {
                                    context.InsertCustomerOrders(orders, maxRetries);
                                    context.CreateBlockOrder(security, maxRetries);
                                }
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
                if (e != null) {
                    Console.WriteLine($"Received {e.GetType()}: {e.Message}");
                }
                else {
                    Console.WriteLine($"Caught an unknown exception");
                }
            }
            finally {
                await Task.Delay(1000, token);
                consumer.Unsubscribe();
                consumer.Close();
            }
        });
        Console.WriteLine($"Completed consumer for {topic}!!");
    }

    private static List<CustomerOrder> GenerateCustomerOrders(
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
                context.InsertCustomerOrders(orders, maxRetries);
                orders = [];
            }
        }
        return orders;
    }
}
