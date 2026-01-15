using DomainUserService.Interfaces.IServices;

namespace GrpcUserService.Grpc;

public class EventPublisherStub : IEventPublisher
{
    public Task PublishUserRegisteredEvent(string userId)
    {
        return Task.CompletedTask;
    }

    public Task PublishPointsSpentEvent(string userId, int amount, string? referenceId)
    {
        return Task.CompletedTask;
    }
}