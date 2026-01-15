using DomainUserService.Models;

namespace DomainUserService.Dto;

public record UserDto(
    Guid Id,
    string Username,
    string PhoneNumber,
    string PasswordHash,
    DateTime RegisteredAt,
    bool IsBlocked,
    int LoyaltyPoints,
    UserRole Role);
