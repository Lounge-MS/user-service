namespace DomainUserService.Models;

public sealed class User(
    Guid id,
    string username,
    string phoneNumber,
    string passwordHash,
    DateTime registeredAt,
    bool isBlocked,
    int loyaltyPoints,
    UserRole role)
{
    public Guid Id { get; set; } = id;

    public string Username { get; set; } = username;

    public string PhoneNumber { get; set; } = phoneNumber;

    public string PasswordHash { get; set; } = passwordHash;

    public DateTime RegisteredAt { get; set; } = registeredAt;

    public bool IsBlocked { get; set; } = isBlocked;

    public int LoyaltyPoints { get; set; } = loyaltyPoints;

    public UserRole Role { get; set; } = role;
}