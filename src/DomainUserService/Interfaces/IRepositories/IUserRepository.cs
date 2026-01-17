using DomainUserService.Models;

namespace DomainUserService.Interfaces.IRepositories;

public interface IUserRepository
{
    Task AddAsync(User user, CancellationToken cancellationToken);

    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken);

    Task UpdateAsync(User user, CancellationToken cancellationToken);

    Task DeleteAsync(string username, CancellationToken cancellationToken);

    Task<(List<User> Users, int TotalCount)> SearchAsync(
        string? query,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
