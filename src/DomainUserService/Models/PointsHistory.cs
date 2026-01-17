using DomainUserService.Dto;

namespace DomainUserService.Models;

public sealed class PointsHistory(
    string id,
    string userId,
    string orderId,
    int points,
    DateTime createdAt,
    PointsTransactionType type)
{
    public string Id { get; set; } = id;

    public string UserId { get; set; } = userId;

    public string OrderId { get; set; } = orderId;

    public int Points { get; set; } = points;

    public DateTime CreatedAt { get; set; } = createdAt;

    public PointsTransactionType Type { get; set; } = type;
}