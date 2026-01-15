using DomainUserService.Dto;

namespace DomainUserService.Models;

public sealed class PointsHistory(
    Guid id,
    Guid userId,
    Guid orderId,
    int points,
    DateTime createdAt,
    PointsTransactionType type)
{
    public Guid Id { get; set; } = id;

    public Guid UserId { get; set; } = userId;

    public Guid OrderId { get; set; } = orderId;

    public int Points { get; set; } = points;

    public DateTime CreatedAt { get; set; } = createdAt;

    public PointsTransactionType Type { get; set; } = type;
}