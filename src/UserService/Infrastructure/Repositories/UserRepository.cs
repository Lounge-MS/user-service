using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UserService.Interfaces.IRepositories;
using UserService.Models;

namespace UserService.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    // Простая in-memory реализация для примера.
    // В реальном проекте здесь будет доступ к БД (EF Core / Dapper и т.д.).
    private static readonly ConcurrentDictionary<Guid, User> _users = new();

    public Task AddAsync(User user, CancellationToken cancellationToken)
    {
        _users[user.Id] = user;
        return Task.CompletedTask;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        _users.TryGetValue(id, out var user);
        return Task.FromResult(user);
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken)
    {
        _users[user.Id] = user;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        _users.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task<(List<User> Users, int TotalCount)> SearchAsync(
        string? query,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        if (page <= 0)
        {
            page = 1;
        }

        if (pageSize <= 0)
        {
            pageSize = 10;
        }

        IEnumerable<User> result = _users.Values;

        if (!string.IsNullOrWhiteSpace(query))
        {
            var q = query.Trim();
            result = result.Where(u =>
                u.Username.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                u.PhoneNumber.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        var totalCount = result.Count();

        var users = result
            .OrderBy(u => u.Username)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult((users, totalCount));
    }
}
