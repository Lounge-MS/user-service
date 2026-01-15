using DomainUserService.Models;

namespace DomainUserService.Dto;

public record CreateUserDto(
    Guid Id,
    string Username,
    string PasswordHash,
    UserRole Role);
