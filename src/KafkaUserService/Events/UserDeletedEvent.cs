namespace KafkaUserService.Events;

public record UserDeletedEvent
{
    public required string UserId { get; init; }

    public required DateTime Timestamp { get; init; }
}