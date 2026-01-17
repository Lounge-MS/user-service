namespace DomainUserService.Dto;

public record PointsHistoryDto(
    string Id,
    string UserId,
    string OrderId,
    int Points,
    DateTime CreatedAt,
    PointsTransactionType Type);
