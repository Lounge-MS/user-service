using DomainUserService.Dto;
using DomainUserService.Interfaces.IRepositories;
using DomainUserService.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace InfrastructureUserService.Infrastructure.Repositories;

public class PointsHistoryRepository : IPointsHistoryRepository, IAsyncDisposable
{
    private readonly NpgsqlConnection _connection;

    public PointsHistoryRepository(string? connectionString)
    {
        _connection = new NpgsqlConnection(connectionString);
    }

    public async Task AddAsync(PointsHistory history, CancellationToken cancellationToken)
    {
        await EnsureConnectionOpenAsync(cancellationToken);

        const string sql = @"
            INSERT INTO points_history (id, user_id, order_id, points, type, created_at)
            VALUES (@Id, @UserId, @OrderId, @Points, @Type, @CreatedAt)";

        using var cmd = new NpgsqlCommand(sql, _connection);
        cmd.Parameters.AddWithValue("@Id", history.Id);
        cmd.Parameters.AddWithValue("@UserId", history.UserId);
        cmd.Parameters.AddWithValue("@OrderId", history.OrderId);
        cmd.Parameters.AddWithValue("@Points", history.Points);
        cmd.Parameters.AddWithValue("@CreatedAt", history.CreatedAt);
        cmd.Parameters.AddWithValue("@Type", history.Type);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<List<PointsHistory>> GetByUserAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;

        await EnsureConnectionOpenAsync(cancellationToken);

        const string sql = @"
            SELECT id, user_id, order_id, points, created_at, type
            FROM points_history
            WHERE user_id = @UserId
            ORDER BY created_at DESC
            LIMIT @PageSize OFFSET @Offset";

        using var cmd = new NpgsqlCommand(sql, _connection);
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@PageSize", pageSize);
        cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);

        var histories = new List<PointsHistory>();
        using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            histories.Add(new PointsHistory(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetInt32(3),
                reader.GetDateTime(4),
                (PointsTransactionType)reader.GetInt32(5)));
        }

        return histories;
    }

    public async Task<int> GetCountByUserAsync(string userId, CancellationToken cancellationToken)
    {
        await EnsureConnectionOpenAsync(cancellationToken);

        const string sql = "SELECT COUNT(*) FROM points_history WHERE user_id = @UserId";

        using var cmd = new NpgsqlCommand(sql, _connection);
        cmd.Parameters.AddWithValue("@UserId", userId);

        object? result = await cmd.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }

    private async Task EnsureConnectionOpenAsync(CancellationToken cancellationToken)
    {
        if (_connection.State != ConnectionState.Open)
        {
            await _connection.OpenAsync(cancellationToken);
        }
    }
}