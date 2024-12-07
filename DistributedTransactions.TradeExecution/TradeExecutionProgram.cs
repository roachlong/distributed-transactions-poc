
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Confluent.Kafka;
using DistributedTransactions.Data;
using DistributedTransactions.Domain.Orders;
using DistributedTransactions.Domain.Trades;
using Microsoft.IdentityModel.Tokens;

namespace DistributedTransactions.TradeExecution;

class TradeExecutionProgram
{
    static void Main(string[] args) {
        MainAsync().Wait();
    }
    
    private static async Task MainAsync() {
        const string topic = "block-orders";
        const int numTasks = 10;
        const int batchSize = 128;
        const int batchWindow = 60000;
        const int maxRetries = 5;
        const int amendRate = 1;
        const int cancelRate = 1;

        CancellationTokenSource cts = new();
        Console.CancelKeyPress += (_, e) => {
            e.Cancel = true;
            cts.Cancel();
        };

        var tasks = new List<Task>();
        for (int i = 0; i < numTasks; i++) {
            tasks.Add(SimulateTrades(
                topic, batchSize, batchWindow, maxRetries, amendRate, cancelRate, cts.Token
            ));
        }
        await Task.WhenAll(tasks);
        cts.Dispose();
    }

    private static async Task SimulateTrades(
        string topic, int batchSize, int batchWindow, int maxRetries,
        int amendRate, int cancelRate, CancellationToken token
    ) {
        Console.WriteLine($"Launching consumer for {topic}...");
        await Task.Run(async () => {
            var random = new Random();
            var orders = new List<TradeOrder>();
            var config = TradeExecutionConfig.GetConfig();
            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            await Task.Delay(1000, token);
            consumer.Subscribe(topic);
            try {
                Stopwatch window = Stopwatch.StartNew();
                while (!token.IsCancellationRequested) {
                    var msg = consumer.Consume(batchWindow);
                    if (!token.IsCancellationRequested && msg != null && msg.Message.Value != null) {
                        var json = JsonObject.Parse(msg.Message.Value);
                        if (json != null && json["after"] != null) {
                            BlockOrder? order = JsonSerializer.Deserialize<BlockOrder>(json["after"]);
                            if (order != null)
                            {
                                using var context = new TradesDbContext();
                                orders.Add(context.GetTradingPriceRange(order, maxRetries));
                                if (orders.Count >= batchSize || window.ElapsedMilliseconds >= batchWindow) {
                                    SimulateTradeOrders(orders, amendRate, cancelRate,
                                                        random, context, maxRetries);
                                    orders = [];
                                    window = Stopwatch.StartNew();
                                }
                            }
                        }
                        consumer.Commit(msg);
                    }
                    if (orders.Count > 0 && window.ElapsedMilliseconds >= batchWindow) {
                        using var context = new TradesDbContext();
                        SimulateTradeOrders(orders, amendRate, cancelRate,
                                            random, context, maxRetries);
                        orders = [];
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
                await Task.Delay(1000, token);
                consumer.Unsubscribe();
                consumer.Close();
            }
        });
        Console.WriteLine($"Completed consumer for {topic}!!");
    }

    private static void SimulateTradeOrders(
        List<TradeOrder> orders, int amendRate, int cancelRate,
        Random random, TradesDbContext context, int maxRetries
    ) {
        var seqNum = 0;
        while (true) {
            seqNum++;
            List<BaseTradeModel> trades = orders
                .Select(o => GetTrade(ref o, amendRate, cancelRate, random, seqNum))
                .OfType<BaseTradeModel>()
                .ToList();
            if (trades.IsNullOrEmpty()) {
                break;
            }
            else {
                List<ReplacedTrade> amended = trades.OfType<ReplacedTrade>().ToList();
                if (!amended.IsNullOrEmpty()) {
                    context.InsertReplacedTrades(amended, maxRetries);
                }
                List<BustedTrade> cancelled = trades.OfType<BustedTrade>().ToList();
                if (!cancelled.IsNullOrEmpty()) {
                    context.InsertBustedTrades(cancelled, maxRetries);
                }
                List<ExecutedTrade> executed = trades.OfType<ExecutedTrade>().ToList();
                if (!executed.IsNullOrEmpty()) {
                    context.InsertExecutedTrades(executed, maxRetries);
                }
            }
        }
    }

    private static BaseTradeModel? GetTrade(
        ref TradeOrder order, int amendRate, int cancelRate, Random random, int seqNum
    ) {
        if (order.Needed > 0) {
            if (amendRate >= random.Next(1, 100)) {
#pragma warning disable CS8605 // Unboxing a possibly null value.
                TradeDestination? newDestination = null;
                if (random.Next(1, 100) <= 50) {
                    Array values = Enum.GetValues(typeof(TradeDestination));
                    var value = random.Next(values.Length);
                    newDestination = (TradeDestination)values.GetValue(value);
                }
                TradeType? newType = null;
                if (random.Next(1, 100) <= 50) {
                    Array values = Enum.GetValues(typeof(TradeType));
                    var value = random.Next(values.Length);
                    newType = (TradeType)values.GetValue(value);
                }
                TradeRestriction? newRestriction = null;
                if (random.Next(1, 100) <= 50) {
                    Array values = Enum.GetValues(typeof(TradeRestriction));
                    var value = random.Next(values.Length);
                    newRestriction = (TradeRestriction)values.GetValue(value);
                }
#pragma warning restore CS8605 // Unboxing a possibly null value.
                int? newQuantity = null;
                if (random.Next(1, 100) <= 50) {
                    var percentIncrease = random.NextDouble();
                    int newIncrease = (int)(order.Quantity * percentIncrease);
                    order.Quantity += newIncrease;
                    order.Needed += newIncrease;
                    newQuantity = order.Quantity;
                }
                return new ReplacedTrade {
                    BlockOrderCode = order.Code,
                    BlockOrderSeqNum = seqNum,
                    AssetClass = order.AssetClass,
                    Symbol = order.Symbol,
                    Date = order.Date,
                    Direction = (TradeDirection) (int) order.Direction,
                    Destination = (TradeDestination) (int) order.Destination,
                    Type = (TradeType) (int) order.Type,
                    Restriction = (TradeRestriction) (int) order.Restriction,
                    NewDestination = newDestination,
                    NewType = newType,
                    NewRestriction = newRestriction,
                    NewQuantity = newQuantity
                };
            }
            else if (cancelRate >= random.Next(1, 100)) {
                var cancelledQuantity = random.Next(1, order.Needed);
                order.Quantity -= cancelledQuantity;
                order.Needed -= cancelledQuantity;
                return new BustedTrade {
                    BlockOrderCode = order.Code,
                    BlockOrderSeqNum = seqNum,
                    AssetClass = order.AssetClass,
                    Symbol = order.Symbol,
                    Date = order.Date,
                    Direction = (TradeDirection) (int) order.Direction,
                    Destination = (TradeDestination) (int) order.Destination,
                    Type = (TradeType) (int) order.Type,
                    Restriction = (TradeRestriction) (int) order.Restriction,
                    CancelledQuantity = cancelledQuantity
                };
            }
            else {
                var tradeQuantity = Math.Min(random.Next(1, 10) * 50, order.Needed);
                order.Needed -= tradeQuantity;
                var price = Math.Round(random.NextDouble() * (order.High - order.Low) + order.Low, 2);
                return new ExecutedTrade {
                    BlockOrderCode = order.Code,
                    BlockOrderSeqNum = seqNum,
                    AssetClass = order.AssetClass,
                    Symbol = order.Symbol,
                    Date = order.Date,
                    Direction = (TradeDirection) (int) order.Direction,
                    Destination = (TradeDestination) (int) order.Destination,
                    Type = (TradeType) (int) order.Type,
                    Restriction = (TradeRestriction) (int) order.Restriction,
                    Quantity = tradeQuantity,
                    Price = price
                };
            }
        }

        return null;
    }
}
