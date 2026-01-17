using DomainUserService.Dto;

namespace DomainUserService.Interfaces.IServices;

public interface IPointsService
{
    Task<int> GetUserPointsAsync(string username, CancellationToken cancellationToken);

    Task<int> AddPointsAsync(
        string username,
        int amount,
        string? description,
        string? referenceId,
        CancellationToken cancellationToken);

    Task<(bool Success, int NewBalance, string? Error)> SpendPointsAsync(
        string username,
        int amount,
        string? referenceId,
        CancellationToken cancellationToken);

    Task<(bool Success, int NewBalance)> CompensatePointsAsync(
        string username,
        int amount,
        string originalTransactionId,
        CancellationToken cancellationToken);

    Task<(List<PointsHistoryDto> History, int TotalCount)> GetPointsHistoryAsync(
        string username,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
