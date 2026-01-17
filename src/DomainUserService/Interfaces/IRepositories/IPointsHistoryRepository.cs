using DomainUserService.Models;

namespace DomainUserService.Interfaces.IRepositories;

public interface IPointsHistoryRepository
{
    Task AddAsync(PointsHistory history, CancellationToken cancellationToken);

    Task<List<PointsHistory>> GetByUserAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<int> GetCountByUserAsync(string userId, CancellationToken cancellationToken);
}
