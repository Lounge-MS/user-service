namespace DomainUserService.Dto;

public class PointsTransaction
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public int Amount { get; set; }

    public PointsTransactionType Type { get; set; }

    public string? Description { get; set; }

    public string? ReferenceId { get; set; }

    public DateTime CreatedAt { get; set; }

    public int BalanceAfter { get; set; }
}