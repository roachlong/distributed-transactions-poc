using Confluent.Kafka;

namespace DistributedTransactions.TradeExecution;

public static class TradeExecutionConfig
{
    public static ConsumerConfig GetConfig() {
        return new ConsumerConfig {
            BootstrapServers = "localhost:9092",
            GroupId = "TradeExecution",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };
    }
}