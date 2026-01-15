using DomainUserService.Interfaces.IRepositories;
using DomainUserService.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace InfrastructureUserService.Infrastructure.Repositories;

public class UserRepository : IUserRepository, IAsyncDisposable
{
    private readonly NpgsqlConnection _connection;

    public UserRepository(string? connectionString)
    {
        _connection = new NpgsqlConnection(connectionString);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await EnsureConnectionOpenAsync(cancellationToken);

        const string sql = @"
            INSERT INTO users (Id, Username, PhoneNumber, Points, CreatedAt, UpdatedAt)
            VALUES (@Id, @Username, @PhoneNumber, @Points, @CreatedAt, @UpdatedAt)";

        using var cmd = new NpgsqlCommand(sql, _connection);
        cmd.Parameters.AddWithValue("@Id", user.Id);
        cmd.Parameters.AddWithValue("@Username", user.Username);
        cmd.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
        cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
        cmd.Parameters.AddWithValue("@RegisteredAt", user.RegisteredAt);
        cmd.Parameters.AddWithValue("@IsBlocked", user.IsBlocked);
        cmd.Parameters.AddWithValue("@LoyaltyPoints", user.LoyaltyPoints);
        cmd.Parameters.AddWithValue("@Role", user.Role);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await EnsureConnectionOpenAsync(cancellationToken);

        const string sql = @"
            SELECT Id, Username, PhoneNumber, Points, CreatedAt, UpdatedAt
            FROM users
            WHERE Id = @Id";

        using var cmd = new NpgsqlCommand(sql, _connection);
        cmd.Parameters.AddWithValue("@Id", id);

        using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            return new User(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetDateTime(4),
                reader.GetBoolean(5),
                reader.GetInt32(6),
                (UserRole)reader.GetInt32(7));
        }

        return null;
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken)
    {
        await EnsureConnectionOpenAsync(cancellationToken);

        const string sql = @"
            UPDATE users 
            SET Username = @Username, 
                PhoneNumber = @PhoneNumber, 
                Points = @Points,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        using var cmd = new NpgsqlCommand(sql, _connection);
        cmd.Parameters.AddWithValue("@Id", user.Id);
        cmd.Parameters.AddWithValue("@Username", user.Username);
        cmd.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
        cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
        cmd.Parameters.AddWithValue("@RegisteredAt", user.RegisteredAt);
        cmd.Parameters.AddWithValue("@IsBlocked", user.IsBlocked);
        cmd.Parameters.AddWithValue("@LoyaltyPoints", user.LoyaltyPoints);
        cmd.Parameters.AddWithValue("@Role", user.Role);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await EnsureConnectionOpenAsync(cancellationToken);

        const string sql = "DELETE FROM users WHERE Id = @Id";

        using var cmd = new NpgsqlCommand(sql, _connection);
        cmd.Parameters.AddWithValue("@Id", id);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<(List<User> Users, int TotalCount)> SearchAsync(
        string? query,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;

        await EnsureConnectionOpenAsync(cancellationToken);

        string whereClause = string.Empty;
        var parameters = new List<NpgsqlParameter>();

        if (!string.IsNullOrWhiteSpace(query))
        {
            whereClause = " WHERE Username ILIKE @Query OR PhoneNumber ILIKE @Query";
            parameters.Add(new NpgsqlParameter("@Query", $"%{query.Trim()}%"));
        }

        string countSql = $"SELECT COUNT(*) FROM users {whereClause}";
        int totalCount = await ExecuteScalarAsync(countSql, parameters, cancellationToken);

        string sql = $@"
            SELECT Id, Username, PhoneNumber, Points, CreatedAt, UpdatedAt
            FROM users
            {whereClause}
            ORDER BY Username
            LIMIT @PageSize OFFSET @Offset";

        parameters.Add(new NpgsqlParameter("@PageSize", pageSize));
        parameters.Add(new NpgsqlParameter("@Offset", (page - 1) * pageSize));

        var users = new List<User>();
        using var cmd = new NpgsqlCommand(sql, _connection);
        cmd.Parameters.AddRange(parameters.ToArray());

        using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            users.Add(new User(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetDateTime(4),
                reader.GetBoolean(5),
                reader.GetInt32(6),
                (UserRole)reader.GetInt32(7)));
        }

        return (users, totalCount);
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }

    private async Task<int> ExecuteScalarAsync(
        string sql,
        List<NpgsqlParameter> parameters,
        CancellationToken cancellationToken)
    {
        // CA2100: Проверьте, принимает ли строка запроса, переданная в
        // "NpgsqlCommand.NpgsqlCommand(string? cmdText, NpgsqlConnection? connection)" в
        // "ExecuteScalarAsync", вводимые пользователем сведения.
        // хз как пофиксить пока что
#pragma warning disable CA2100
        using var cmd = new NpgsqlCommand(sql, _connection);
#pragma warning restore CA2100
        cmd.Parameters.AddRange(parameters.ToArray());

        object? result = await cmd.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result);
    }

    private async Task EnsureConnectionOpenAsync(CancellationToken cancellationToken)
    {
        if (_connection.State != ConnectionState.Open)
        {
            await _connection.OpenAsync(cancellationToken);
        }
    }
}