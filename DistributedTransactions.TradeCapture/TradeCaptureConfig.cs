using Confluent.Kafka;

namespace DistributedTransactions.TradeCapture;

public static class TradeCaptureConfig
{
    public static ConsumerConfig GetConfig(int partition) {
        return new ConsumerConfig {
            BootstrapServers = "localhost:9092",
            GroupId = $"TradeCapture-{partition}",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };
    }
}