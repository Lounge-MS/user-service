namespace KafkaUserService.Events;

public record PointsCompensatedEvent
{
    public required string UserId { get; init; }

    public required int Amount { get; init; }

    public required string OriginalTransactionId { get; init; }

    public required DateTime Timestamp { get; init; }
}