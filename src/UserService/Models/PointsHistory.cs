using UserService.Dto;

namespace UserService.Models;

public sealed class PointsHistory(
    Guid id,
    Guid userId,
    Guid orderId,
    int points,
    DateTime createdAt,
    PointsTransactionType type)
{
    public Guid Id { get; private set; } = id;
    public Guid UserId { get; private set; } = userId;
    public Guid OrderId { get; private set; } = orderId;
    public int Points { get; private set; } = points;
    public DateTime CreatedAt { get; private set; } = createdAt;
    public PointsTransactionType Type { get; private set; } = type;
}
