using Confluent.Kafka;

namespace DistributedTransactions.RebalanceRequest;

public static class RebalanceRequestConfig
{
    public static ConsumerConfig GetConfig() {
        return new ConsumerConfig {
            BootstrapServers = "localhost:9092",
            GroupId = "RebalanceRequest",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };
    }
}