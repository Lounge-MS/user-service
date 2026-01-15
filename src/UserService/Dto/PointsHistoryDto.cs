namespace UserService.Dto;

public record PointsHistoryDto(
    Guid Id,
    Guid UserId,
    Guid OrderId,
    int Points,
    DateTime CreatedAt,
    PointsTransactionType Type);
