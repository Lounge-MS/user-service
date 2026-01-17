using DomainUserService.Models;

namespace DomainUserService.Dto;

public record UserDto(
    string Id,
    string Username,
    string PasswordHash,
    DateTime RegisteredAt,
    bool IsBlocked,
    int LoyaltyPoints,
    UserRole Role);
