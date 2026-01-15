using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UserService.Interfaces.IRepositories;
using UserService.Models;

namespace UserService.Infrastructure.Repositories;

public class PointsHistoryRepository : IPointsHistoryRepository
{
    // In-memory хранилище истории по пользователям
    private static readonly ConcurrentDictionary<Guid, List<PointsHistory>> _historyByUser = new();

    public Task AddAsync(PointsHistory history, CancellationToken cancellationToken)
    {
        var list = _historyByUser.GetOrAdd(history.UserId, _ => new List<PointsHistory>());
        lock (list)
        {
            list.Add(history);
        }

        return Task.CompletedTask;
    }

    public Task<List<PointsHistory>> GetByUserAsync(
        Guid userId,
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

        if (!_historyByUser.TryGetValue(userId, out var list))
        {
            return Task.FromResult(new List<PointsHistory>());
        }

        List<PointsHistory> snapshot;
        lock (list)
        {
            snapshot = list
                .OrderByDescending(h => h.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        return Task.FromResult(snapshot);
    }

    public Task<int> GetCountByUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (!_historyByUser.TryGetValue(userId, out var list))
        {
            return Task.FromResult(0);
        }

        int count;
        lock (list)
        {
            count = list.Count;
        }

        return Task.FromResult(count);
    }
}

