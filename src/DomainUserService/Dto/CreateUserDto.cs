using DomainUserService.Models;

namespace DomainUserService.Dto;

public record CreateUserDto(
    string Id,
    string Username,
    string PasswordHash,
    UserRole Role);
