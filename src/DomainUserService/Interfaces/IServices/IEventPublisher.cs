namespace DomainUserService.Interfaces.IServices;

public interface IEventPublisher
{
    Task PublishUserRegisteredEvent(string userId);

    Task PublishUserDeletedEvent(string userId);

    Task PublishPointsSpentEvent(string userId, int amount, string? referenceId);

    Task PublishPointsAddedEvent(string userId, int amount, string? referenceId);

    Task PublishPointsCompensatedEvent(string userId, int amount, string originalTransactionId);
}