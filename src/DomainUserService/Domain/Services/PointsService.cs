using DomainUserService.Dto;
using DomainUserService.Interfaces.IRepositories;
using DomainUserService.Interfaces.IServices;
using DomainUserService.Models;
using Microsoft.Extensions.Logging;

namespace DomainUserService.Domain.Services;

public sealed class PointsService(
    IPointsHistoryRepository history,
    IUserRepository users,
    ILogger<PointsService> logger) : IPointsService
{
    private readonly IPointsHistoryRepository _history = history;

    private readonly IUserRepository _users = users;

    private readonly ILogger<PointsService> _logger = logger;

    public async Task<int> GetUserPointsAsync(string username, CancellationToken cancellationToken)
    {
        User user = await GetUserOrThrowAsync(username, cancellationToken);
        return user.LoyaltyPoints;
    }

    public async Task<int> AddPointsAsync(
        string username,
        int amount,
        string? description,
        string? referenceId,
        CancellationToken cancellationToken)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive");
        }

        User user = await GetUserOrThrowAsync(username, cancellationToken);
        user.LoyaltyPoints += amount;
        await _users.UpdateAsync(user, cancellationToken);

        Guid orderId = TryParseGuid(referenceId);

        var history = new PointsHistory(
            id: Guid.NewGuid(),
            userId: user.Id,
            orderId: orderId,
            points: amount,
            createdAt: DateTime.UtcNow,
            type: PointsTransactionType.Add);

        await _history.AddAsync(history, cancellationToken);

        _logger.LogInformation(
            "Added {Amount} points for user {UserId}, new balance {Balance}",
            amount,
            user.Id,
            user.LoyaltyPoints);

        return user.LoyaltyPoints;
    }

    public async Task<(bool Success, int NewBalance, string? Error)> SpendPointsAsync(
        string username,
        int amount,
        string? referenceId,
        CancellationToken cancellationToken)
    {
        if (amount <= 0)
        {
            return (false, 0, "Amount must be positive");
        }

        User user = await GetUserOrThrowAsync(username, cancellationToken);
        if (user.LoyaltyPoints < amount)
        {
            return (false, user.LoyaltyPoints, "Insufficient points");
        }

        user.LoyaltyPoints -= amount;
        await _users.UpdateAsync(user, cancellationToken);

        Guid orderId = TryParseGuid(referenceId);

        var history = new PointsHistory(
            id: Guid.NewGuid(),
            userId: user.Id,
            orderId: orderId,
            points: -amount,
            createdAt: DateTime.UtcNow,
            type: PointsTransactionType.Spend);

        await _history.AddAsync(history, cancellationToken);

        _logger.LogInformation(
            "Spent {Amount} points for user {UserId}, new balance {Balance}",
            amount,
            user.Id,
            user.LoyaltyPoints);

        return (true, user.LoyaltyPoints, null);
    }

    public async Task<(bool Success, int NewBalance)> CompensatePointsAsync(
        string username,
        int amount,
        string originalTransactionId,
        CancellationToken cancellationToken)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive");
        }

        User user = await GetUserOrThrowAsync(username, cancellationToken);

        user.LoyaltyPoints += amount;
        await _users.UpdateAsync(user, cancellationToken);

        Guid orderId = TryParseGuid(originalTransactionId);

        var history = new PointsHistory(
            id: Guid.NewGuid(),
            userId: user.Id,
            orderId: orderId,
            points: amount,
            createdAt: DateTime.UtcNow,
            type: PointsTransactionType.Compensate);

        await _history.AddAsync(history, cancellationToken);

        _logger.LogInformation(
            "Compensated {Amount} points for user {UserId}, new balance {Balance} (original transaction {OriginalTransactionId})",
            amount,
            user.Id,
            user.LoyaltyPoints,
            originalTransactionId);

        return (true, user.LoyaltyPoints);
    }

    public async Task<(List<PointsHistoryDto> History, int TotalCount)> GetPointsHistoryAsync(
        string username,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        User user = await GetUserOrThrowAsync(username, cancellationToken);
        List<PointsHistory> history = await _history.GetByUserAsync(user.Id, page, pageSize, cancellationToken);
        int totalCount = await _history.GetCountByUserAsync(user.Id, cancellationToken);

        var dtos = history
            .Select(h => new PointsHistoryDto(
                Id: h.Id,
                UserId: h.UserId,
                OrderId: h.OrderId,
                Points: h.Points,
                CreatedAt: h.CreatedAt,
                Type: h.Type))
            .ToList();

        return (dtos, totalCount);
    }

    private static Guid TryParseGuid(string? value)
    {
        return Guid.TryParse(value, out Guid guid) ? guid : Guid.Empty;
    }

    private async Task<User> GetUserOrThrowAsync(string username, CancellationToken cancellationToken)
    {
        return await _users.GetByUsernameAsync(username, cancellationToken)
            ?? throw new KeyNotFoundException($"User with username {username} not found");
    }
}
