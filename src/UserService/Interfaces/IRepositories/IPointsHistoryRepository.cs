using UserService.Models;

namespace UserService.Interfaces.IRepositories;

public interface IPointsHistoryRepository
{
    Task AddAsync(PointsHistory history, CancellationToken cancellationToken);

    Task<List<PointsHistory>> GetByUserAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<int> GetCountByUserAsync(Guid userId, CancellationToken cancellationToken);
}
