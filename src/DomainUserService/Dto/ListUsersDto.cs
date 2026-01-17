namespace DomainUserService.Dto;

public record ListUsersDto(
    IList<UserDto> Users,
    int TotalCount);