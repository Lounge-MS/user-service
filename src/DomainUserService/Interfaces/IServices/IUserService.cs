using DomainUserService.Dto;
using DomainUserService.Models;

namespace DomainUserService.Interfaces.IServices;

public interface IUserService
{
    Task<UserDto> CreateUserAsync(CreateUserDto dto, CancellationToken cancellationToken);

    Task<UserDto> DeleteUserAsync(string username, CancellationToken cancellationToken);

    Task<UserDto> GetUserAsync(string username, CancellationToken cancellationToken);

    Task<UserDto> UpdateUserAsync(UserDto dto, CancellationToken cancellationToken);

    Task<UserDto> UpdateUserRoleAsync(string username, UserRole role, CancellationToken cancellationToken);

    Task<UserDto> BlockUserAsync(string username, CancellationToken cancellationToken);

    Task<UserDto> UnblockUserAsync(string username, CancellationToken cancellationToken);

    Task<(List<UserDto> Users, int TotalCount)> ListUsersAsync(string query, int page, int pageSize, CancellationToken cancellationToken);
}
