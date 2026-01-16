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
}