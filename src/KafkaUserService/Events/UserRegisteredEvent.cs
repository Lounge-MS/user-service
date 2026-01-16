namespace KafkaUserService.Events;

public record UserRegisteredEvent
{
    public required string UserId { get; init; }

    public required DateTime Timestamp { get; init; }
}