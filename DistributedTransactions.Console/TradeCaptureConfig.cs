using Confluent.Kafka;

namespace DistributedTransactions.Allocations;

public static class TradeCaptureConfig
{
    public static ConsumerConfig GetConfig() {
        return new ConsumerConfig {
            BootstrapServers = "localhost:9092",
            GroupId = "Test_4_TradeCapture",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };
    }
}