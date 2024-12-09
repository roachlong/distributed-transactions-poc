using Confluent.Kafka;

namespace DistributedTransactions.SecurityRequest;

public static class SecurityRequestConfig
{
    public static ConsumerConfig GetConfig() {
        return new ConsumerConfig {
            BootstrapServers = "localhost:9092",
            GroupId = "SecurityRequest",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };
    }
}