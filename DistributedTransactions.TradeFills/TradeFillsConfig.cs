using Confluent.Kafka;

namespace DistributedTransactions.TradeFills;

public static class TradeFillsConfig
{
    public static ConsumerConfig GetConfig(int partition) {
        return new ConsumerConfig {
            BootstrapServers = "localhost:9092",
            GroupId = $"TradeFills-{partition}",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            EnableAutoOffsetStore = false
        };
    }
}