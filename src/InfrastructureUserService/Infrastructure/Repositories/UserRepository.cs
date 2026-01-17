using DomainUserService.Exceptions;
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
            INSERT INTO users (id, username, phone_number, password_hash, registered_at, is_blocked, loyalty_points, role)
            VALUES (@Id, @Username, @PhoneNumber, @PasswordHash, @RegisteredAt, @IsBlocked, @LoyaltyPoints, @Role)";

        using var cmd = new NpgsqlCommand(sql, _connection);
        cmd.Parameters.AddWithValue("@Id", user.Id);
        cmd.Parameters.AddWithValue("@Username", user.Username);
        cmd.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
        cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
        cmd.Parameters.AddWithValue("@RegisteredAt", user.RegisteredAt);
        cmd.Parameters.AddWithValue("@IsBlocked", user.IsBlocked);
        cmd.Parameters.AddWithValue("@LoyaltyPoints", user.LoyaltyPoints);
        cmd.Parameters.AddWithValue("@Role", (int)user.Role);

        try
        {
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (PostgresException ex)
        {
            if (ex.SqlState is "23505")
            {
                throw new CollisionException();
            }
        }
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        await EnsureConnectionOpenAsync(cancellationToken);

        const string sql = @"
            SELECT username, username, phone_number, password_hash, registered_at, is_blocked, loyalty_points, role
            FROM users
            WHERE username = @Username";

        using var cmd = new NpgsqlCommand(sql, _connection);
        cmd.Parameters.AddWithValue("@Username", username);

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
            SET username = @Username, 
                phone_number = @PhoneNumber,
                password_hash = @PasswordHash,
                is_blocked = @IsBlocked,
                loyalty_points = @LoyaltyPoints,
                role = @Role
            WHERE id = @Id";

        using var cmd = new NpgsqlCommand(sql, _connection);
        cmd.Parameters.AddWithValue("@Id", user.Id);
        cmd.Parameters.AddWithValue("@Username", user.Username);
        cmd.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
        cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
        cmd.Parameters.AddWithValue("@IsBlocked", user.IsBlocked);
        cmd.Parameters.AddWithValue("@LoyaltyPoints", user.LoyaltyPoints);
        cmd.Parameters.AddWithValue("@Role", (int)user.Role);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(string username, CancellationToken cancellationToken)
    {
        await EnsureConnectionOpenAsync(cancellationToken);

        const string sql = "DELETE FROM users WHERE username = @Username";

        using var cmd = new NpgsqlCommand(sql, _connection);
        cmd.Parameters.AddWithValue("@Username", username);

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

        int totalCount;
        const string countSqlWithoutFilter = "SELECT COUNT(*) FROM users";
        const string countSqlWithFilter =
            "SELECT COUNT(*) FROM users WHERE username ILIKE @Query OR phone_number ILIKE @Query";

        if (!string.IsNullOrWhiteSpace(query))
        {
            using var countCmd = new NpgsqlCommand(countSqlWithFilter, _connection);
            countCmd.Parameters.AddWithValue("@Query", $"%{query.Trim()}%");
            object? countResult = await countCmd.ExecuteScalarAsync(cancellationToken);
            totalCount = Convert.ToInt32(countResult);
        }
        else
        {
            using var countCmd = new NpgsqlCommand(countSqlWithoutFilter, _connection);
            object? countResult = await countCmd.ExecuteScalarAsync(cancellationToken);
            totalCount = Convert.ToInt32(countResult);
        }

        const string sqlWithoutFilter = @"
            SELECT id, username, phone_number, password_hash, registered_at, is_blocked, loyalty_points, role
            FROM users
            ORDER BY username
            LIMIT @PageSize OFFSET @Offset";

        const string sqlWithFilter = @"
            SELECT id, username, phone_number, password_hash, registered_at, is_blocked, loyalty_points, role
            FROM users
            WHERE username ILIKE @Query OR phone_number ILIKE @Query
            ORDER BY username
            LIMIT @PageSize OFFSET @Offset";

        var users = new List<User>();
        string sql = string.IsNullOrWhiteSpace(query) ? sqlWithoutFilter : sqlWithFilter;
        using var cmd = new NpgsqlCommand(sql, _connection);

        if (!string.IsNullOrWhiteSpace(query))
        {
            cmd.Parameters.AddWithValue("@Query", $"%{query.Trim()}%");
        }

        cmd.Parameters.AddWithValue("@PageSize", pageSize);
        cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);

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

    private async Task EnsureConnectionOpenAsync(CancellationToken cancellationToken)
    {
        if (_connection.State != ConnectionState.Open)
        {
            await _connection.OpenAsync(cancellationToken);
        }
    }
}