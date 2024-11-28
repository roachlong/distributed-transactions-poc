using Confluent.Kafka;

namespace DistributedTransactions.Allocations;

public static class TradeCaptureConfig
{
    public static ConsumerConfig GetConfig() {
        return new ConsumerConfig {
            BootstrapServers = "localhost:9092",
            GroupId = "TradeCapture",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };
    }
}