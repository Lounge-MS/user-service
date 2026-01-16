namespace KafkaUserService.Configuration;

public sealed class KafkaOptions
{
    public static string SectionName { get; set; } = "Kafka";

    public required string BootstrapServers { get; init; }

    public required string UserRegisteredTopic { get; init; }

    public required string UserDeletedTopic { get; init; }

    public required string PointsSpentTopic { get; init; }

    public required string PointsAddedTopic { get; init; }

    public required string PointsCompensatedTopic { get; init; }

    public string Acks { get; init; } = "all";

    public bool EnableIdempotence { get; init; } = true;

    public int MaxInFlight { get; init; } = 5;

    public int RetryBackoffMs { get; init; } = 100;

    public int MessageSendMaxRetries { get; init; } = 3;
}