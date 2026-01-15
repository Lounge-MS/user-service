using Microsoft.Extensions.Logging;
using UserService.Dto;
using UserService.Interfaces.IRepositories;
using UserService.Interfaces.IServices;
using UserService.Models;

namespace UserService.Domain.Services;

public sealed class PointsService(
    IPointsHistoryRepository history,
    IUserRepository users,
    ILogger<PointsService> logger) : IPointsService
{
    private readonly IPointsHistoryRepository _history = history;

    private readonly IUserRepository _users = users;

    private readonly ILogger<PointsService> _logger = logger;

    public async Task<int> GetUserPointsAsync(Guid userId, CancellationToken cancellationToken)
    {
        User user = await _users.GetByIdAsync(userId, cancellationToken)
                   ?? throw new KeyNotFoundException($"User with id {userId} not found");

        return user.LoyaltyPoints;
    }

    public async Task<int> AddPointsAsync(
        Guid userId,
        int amount,
        string? description,
        string? referenceId,
        CancellationToken cancellationToken)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive");
        }

        User user = await _users.GetByIdAsync(userId, cancellationToken)
                   ?? throw new KeyNotFoundException($"User with id {userId} not found");

        user.LoyaltyPoints += amount;
        await _users.UpdateAsync(user, cancellationToken);

        Guid orderId = TryParseGuid(referenceId);

        var history = new PointsHistory(
            id: Guid.NewGuid(),
            userId: userId,
            orderId: orderId,
            points: amount,
            createdAt: DateTime.UtcNow,
            type: PointsTransactionType.Add);

        await _history.AddAsync(history, cancellationToken);

        _logger.LogInformation(
            "Added {Amount} points for user {UserId}, new balance {Balance}",
            amount,
            userId,
            user.LoyaltyPoints);

        return user.LoyaltyPoints;
    }

    public async Task<(bool Success, int NewBalance, string? Error)> SpendPointsAsync(
        Guid userId,
        int amount,
        string? referenceId,
        CancellationToken cancellationToken)
    {
        if (amount <= 0)
        {
            return (false, 0, "Amount must be positive");
        }

        User? user = await _users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return (false, 0, "User not found");
        }

        if (user.LoyaltyPoints < amount)
        {
            return (false, user.LoyaltyPoints, "Insufficient points");
        }

        user.LoyaltyPoints -= amount;
        await _users.UpdateAsync(user, cancellationToken);

        Guid orderId = TryParseGuid(referenceId);

        var history = new PointsHistory(
            id: Guid.NewGuid(),
            userId: userId,
            orderId: orderId,
            points: -amount,
            createdAt: DateTime.UtcNow,
            type: PointsTransactionType.Spend);

        await _history.AddAsync(history, cancellationToken);

        _logger.LogInformation(
            "Spent {Amount} points for user {UserId}, new balance {Balance}",
            amount,
            userId,
            user.LoyaltyPoints);

        return (true, user.LoyaltyPoints, null);
    }

    public async Task<(bool Success, int NewBalance)> CompensatePointsAsync(
        Guid userId,
        int amount,
        string originalTransactionId,
        CancellationToken cancellationToken)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive");
        }

        User user = await _users.GetByIdAsync(userId, cancellationToken)
                   ?? throw new KeyNotFoundException($"User with id {userId} not found");

        user.LoyaltyPoints += amount;
        await _users.UpdateAsync(user, cancellationToken);

        Guid orderId = TryParseGuid(originalTransactionId);

        var history = new PointsHistory(
            id: Guid.NewGuid(),
            userId: userId,
            orderId: orderId,
            points: amount,
            createdAt: DateTime.UtcNow,
            type: PointsTransactionType.Compensate);

        await _history.AddAsync(history, cancellationToken);

        _logger.LogInformation(
            "Compensated {Amount} points for user {UserId}, new balance {Balance} (original transaction {OriginalTransactionId})",
            amount,
            userId,
            user.LoyaltyPoints,
            originalTransactionId);

        return (true, user.LoyaltyPoints);
    }

    public async Task<(List<PointsHistoryDto> History, int TotalCount)> GetPointsHistoryAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        List<PointsHistory> history = await _history.GetByUserAsync(userId, page, pageSize, cancellationToken);
        int totalCount = await _history.GetCountByUserAsync(userId, cancellationToken);

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
}
