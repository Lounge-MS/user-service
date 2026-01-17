using DomainUserService.Dto;
using DomainUserService.Interfaces.IRepositories;
using DomainUserService.Interfaces.IServices;
using DomainUserService.Models;
using Microsoft.Extensions.Logging;

namespace DomainUserService.Domain.Services;

public sealed class UserService(
    IUserRepository users,
    ILogger<UserService> logger) : IUserService
{
    public async Task<UserDto> CreateUserAsync(CreateUserDto dto, CancellationToken cancellationToken)
    {
        UserRole role = dto.Role;
        if (!Enum.IsDefined(role))
        {
            role = UserRole.GUEST;
        }

        var user = new User(
            dto.Id,
            dto.Username,
            phoneNumber: string.Empty,
            passwordHash: dto.PasswordHash,
            registeredAt: DateTime.UtcNow,
            isBlocked: false,
            loyaltyPoints: 0,
            role: role);

        await users.AddAsync(user, cancellationToken);

        logger.LogInformation("User created with Id {UserId}", user.Id);

        return ToDto(user);
    }

    public async Task<UserDto> DeleteUserAsync(string username, CancellationToken cancellationToken)
    {
        User user = await GetUserOrThrowAsync(username, cancellationToken);

        await users.DeleteAsync(username, cancellationToken);

        return ToDto(user);
    }

    public async Task<UserDto> GetUserAsync(string username, CancellationToken cancellationToken)
    {
        User user = await GetUserOrThrowAsync(username, cancellationToken);
        return ToDto(user);
    }

    public async Task<UserDto> UpdateUserAsync(UserDto dto, CancellationToken cancellationToken)
    {
        User user = await GetUserOrThrowAsync(dto.Username, cancellationToken);
        user.Username = dto.Username;

        if (!string.IsNullOrWhiteSpace(dto.PasswordHash))
        {
            user.PasswordHash = dto.PasswordHash;
        }

        user.IsBlocked = dto.IsBlocked;
        user.LoyaltyPoints = dto.LoyaltyPoints;
        user.Role = dto.Role;

        await users.UpdateAsync(user, cancellationToken);

        return ToDto(user);
    }

    public async Task<UserDto> UpdateUserRoleAsync(string username, UserRole role, CancellationToken cancellationToken)
    {
        User user = await GetUserOrThrowAsync(username, cancellationToken);

        if (!Enum.IsDefined(role))
        {
            role = UserRole.GUEST;
        }

        user.Role = role;

        await users.UpdateAsync(user, cancellationToken);

        return ToDto(user);
    }

    public async Task<UserDto> BlockUserAsync(string username, CancellationToken cancellationToken)
    {
        User user = await GetUserOrThrowAsync(username, cancellationToken);

        user.IsBlocked = true;

        await users.UpdateAsync(user, cancellationToken);

        return ToDto(user);
    }

    public async Task<UserDto> UnblockUserAsync(string username, CancellationToken cancellationToken)
    {
        User user = await GetUserOrThrowAsync(username, cancellationToken);

        user.IsBlocked = false;

        await users.UpdateAsync(user, cancellationToken);

        return ToDto(user);
    }

    public async Task<(List<UserDto> Users, int TotalCount)> ListUsersAsync(
        string query,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        (List<User> users1, int totalCount) = await users.SearchAsync(query, page, pageSize, cancellationToken);

        var dtos = users1
            .Select(ToDto)
            .ToList();

        return (dtos, totalCount);
    }

    private static UserDto ToDto(User user)
    {
        return new UserDto(
            Id: user.Id,
            Username: user.Username,
            PasswordHash: user.PasswordHash,
            RegisteredAt: user.RegisteredAt,
            IsBlocked: user.IsBlocked,
            LoyaltyPoints: user.LoyaltyPoints,
            Role: user.Role);
    }

    private async Task<User> GetUserOrThrowAsync(string username, CancellationToken cancellationToken)
    {
        return await users.GetByUsernameAsync(username, cancellationToken)
            ?? throw new KeyNotFoundException($"User with username {username} not found");
    }
}
