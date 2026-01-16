namespace KafkaUserService.Events;

public record PointsAddedEvent
{
    public required string UserId { get; init; }

    public required int Amount { get; init; }

    public string? ReferenceId { get; init; }

    public required DateTime Timestamp { get; init; }
}