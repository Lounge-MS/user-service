namespace DomainUserService.Interfaces.IServices;

public interface IEventPublisher
{
    Task PublishUserRegisteredEvent(string userId);

    Task PublishPointsSpentEvent(string userId, int amount, string? referenceId);
}