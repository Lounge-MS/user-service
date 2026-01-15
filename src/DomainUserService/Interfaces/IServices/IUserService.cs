using DomainUserService.Dto;
using DomainUserService.Models;

namespace DomainUserService.Interfaces.IServices;

public interface IUserService
{
    Task<UserDto> CreateUserAsync(CreateUserDto dto, CancellationToken cancellationToken);

    Task<UserDto> DeleteUserAsync(Guid id, CancellationToken cancellationToken);

    Task<UserDto> GetUserAsync(Guid id, CancellationToken cancellationToken);

    Task<UserDto> UpdateUserAsync(UserDto dto, CancellationToken cancellationToken);

    Task<UserDto> UpdateUserRoleAsync(Guid id, UserRole role, CancellationToken cancellationToken);

    Task<UserDto> BlockUserAsync(Guid id, CancellationToken cancellationToken);

    Task<UserDto> UnblockUserAsync(Guid id, CancellationToken cancellationToken);

    Task<(List<UserDto> Users, int TotalCount)> ListUsersAsync(string query, int page, int pageSize, CancellationToken cancellationToken);
}
