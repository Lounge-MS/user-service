using UserService.Dto;

namespace UserService.Interfaces.IServices;

public interface IPointsService
{
    Task<int> GetUserPointsAsync(Guid userId, CancellationToken cancellationToken);

    Task<int> AddPointsAsync(
        Guid userId,
        int amount,
        string? description,
        string? referenceId,
        CancellationToken cancellationToken);

    Task<(bool Success, int NewBalance, string? Error)> SpendPointsAsync(
        Guid userId,
        int amount,
        string? referenceId,
        CancellationToken cancellationToken);

    Task<(bool Success, int NewBalance)> CompensatePointsAsync(
        Guid userId,
        int amount,
        string originalTransactionId,
        CancellationToken cancellationToken);

    Task<(List<PointsHistoryDto> History, int TotalCount)> GetPointsHistoryAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
