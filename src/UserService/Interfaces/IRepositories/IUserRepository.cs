using UserService.Models;

namespace UserService.Interfaces.IRepositories;

public interface IUserRepository
{
    Task AddAsync(User user, CancellationToken cancellationToken);

    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task UpdateAsync(User user, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<(List<User> Users, int TotalCount)> SearchAsync(
        string? query,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
