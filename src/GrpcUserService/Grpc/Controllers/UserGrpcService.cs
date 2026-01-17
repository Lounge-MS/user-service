using DomainUserService.Dto;
using DomainUserService.Interfaces.IServices;
using Grpc.Core;
using GrpcUserService.Grpc.Mappers;
using GrpcUserService.Grpc.Protos;

namespace GrpcUserService.Grpc.Controllers;

public class UserGrpcService : UserService.UserServiceBase
{
    private readonly IUserService _userService;
    private readonly IPointsService _pointsService;
    private readonly IEventPublisher _eventPublisher;

    public UserGrpcService(
        IUserService userService,
        IPointsService pointsService,
        IEventPublisher eventPublisher)
    {
        _userService = userService;
        _pointsService = pointsService;
        _pointsService = pointsService;
        _eventPublisher = eventPublisher;
    }

    public override async Task<UserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.Id)
            || string.IsNullOrWhiteSpace(request.Password)
            || string.IsNullOrWhiteSpace(request.Username)
            || request.Role == UserRole.Unspecified)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Empty value"));
        }

        var createUserDto = UserMapper.ToCreateUserDto(request);

        UserDto userDto = await _userService.CreateUserAsync(
            createUserDto,
            context.CancellationToken);

        await _eventPublisher.PublishUserRegisteredEvent(
            userDto.Id.ToString());

        return new UserResponse
        {
            User = UserMapper.ToProtoUser(userDto),
        };
    }

    public override async Task<UserResponse> GetUser(GetUserRequest request, ServerCallContext context)
    {
        UserDto userDto = await _userService.GetUserAsync(
            request.Username,
            context.CancellationToken);

        return new UserResponse
        {
            User = UserMapper.ToProtoUser(userDto),
        };
    }

    public override async Task<UserResponse> UpdateUser(UpdateUserRequest request, ServerCallContext context)
    {
        string userId = ParseOrThrowGuid(request.UserId);

        UserDto existingUser = await _userService.GetUserAsync(
            request.Username,
            context.CancellationToken);

        UserDto updateDto = existingUser with
        {
            Id = userId,
            Username = request.Username ?? existingUser.Username,
            PasswordHash = request.Password,
        };

        UserDto userDto = await _userService.UpdateUserAsync(
            updateDto,
            context.CancellationToken);

        return new UserResponse
        {
            User = UserMapper.ToProtoUser(userDto),
        };
    }

    public override async Task<UserResponse> UpdateUserRole(UpdateUserRoleRequest request, ServerCallContext context)
    {
        UserDto userDto = await _userService.UpdateUserRoleAsync(
            request.Username,
            UserMapper.ToDomainRole(request.Role),
            context.CancellationToken);

        return new UserResponse
        {
            User = UserMapper.ToProtoUser(userDto),
        };
    }

    public override async Task<UserResponse> BlockUser(BlockUserRequest request, ServerCallContext context)
    {
        UserDto userDto = await _userService.BlockUserAsync(
            request.Username,
            context.CancellationToken);

        return new UserResponse
        {
            User = UserMapper.ToProtoUser(userDto),
        };
    }

    public override async Task<UserResponse> UnblockUser(UnblockUserRequest request, ServerCallContext context)
    {
        UserDto userDto = await _userService.UnblockUserAsync(
            request.Username,
            context.CancellationToken);

        return new UserResponse
        {
            User = UserMapper.ToProtoUser(userDto),
        };
    }

    public override async Task<UserResponse> DeleteUser(DeleteUserRequest request, ServerCallContext context)
    {
        UserDto userDto = await _userService.DeleteUserAsync(
            request.Username,
            context.CancellationToken);

        await _eventPublisher.PublishUserDeletedEvent(
            request.Username);

        return new UserResponse
        {
            User = UserMapper.ToProtoUser(userDto),
        };
    }

    public override async Task<ListUsersResponse> ListUsers(ListUsersRequest request, ServerCallContext context)
    {
        ListUsersDto listusers = await _userService.ListUsersAsync(
            request.Query,
            request.Page,
            request.PageSize,
            context.CancellationToken);

        var response = new ListUsersResponse
        {
            TotalCount = listusers.TotalCount,
            Page = request.Page,
            PageSize = request.PageSize,
        };

        response.Users.AddRange(listusers.Users.Select(UserMapper.ToProtoUser));

        return response;
    }

    public override async Task<PointsResponse> GetUserPoints(GetUserPointsRequest request, ServerCallContext context)
    {
        string userId = ParseOrThrowGuid(request.UserId);

        int points = await _pointsService.GetUserPointsAsync(
            request.UserId,
            context.CancellationToken);

        return new PointsResponse
        {
            UserId = request.UserId,
            Points = points,
        };
    }

    public override async Task<SpendPointsResponse> SpendPoints(SpendPointsRequest request, ServerCallContext context)
    {
        string userId = ParseOrThrowGuid(request.UserId);

        (bool Success, int NewBalance, string? Error) result = await _pointsService.SpendPointsAsync(
            userId,
            request.Amount,
            request.ReferenceId,
            context.CancellationToken);

        if (!result.Success)
        {
            throw new RpcException(new Status(StatusCode.Internal, result.Error ?? "Internal error"));
        }

        await _eventPublisher.PublishPointsSpentEvent(
            request.UserId,
            request.Amount,
            request.ReferenceId);

        return new SpendPointsResponse
        {
            UserId = request.UserId,
            NewBalance = result.NewBalance,
        };
    }

    public override async Task<PointsResponse> AddPoints(AddPointsRequest request, ServerCallContext context)
    {
        string userId = ParseOrThrowGuid(request.UserId);

        if (request.Amount <= 0)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Negative amount"));
        }

        int newBalance = await _pointsService.AddPointsAsync(
            userId,
            request.Amount,
            request.Description,
            request.ReferenceId,
            context.CancellationToken);

        await _eventPublisher.PublishPointsAddedEvent(
            request.UserId,
            request.Amount,
            request.ReferenceId);

        return new PointsResponse
        {
            UserId = request.UserId,
            Points = newBalance,
        };
    }

    public override async Task<CompensatePointsResponse> CompensatePoints(
        CompensatePointsRequest request,
        ServerCallContext context)
    {
        string userId = ParseOrThrowGuid(request.UserId);

        if (request.Amount <= 0)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Negative amount"));
        }

        (bool Success, int NewBalance) result = await _pointsService.CompensatePointsAsync(
            userId,
            request.Amount,
            request.OriginalTransactionId,
            context.CancellationToken);

        if (result.Success)
        {
            await _eventPublisher.PublishPointsCompensatedEvent(
                request.UserId,
                request.Amount,
                request.OriginalTransactionId);
        }

        return new CompensatePointsResponse
        {
            UserId = request.UserId,
            NewBalance = result.NewBalance,
        };
    }

    public override async Task<PointsHistoryResponse> GetPointsHistory(
        GetPointsHistoryRequest request,
        ServerCallContext context)
    {
        string userId = ParseOrThrowGuid(request.UserId);

        int page = request.Page > 0 ? request.Page : 1;
        int pageSize = request.PageSize > 0 ? request.PageSize : 10;

        (List<PointsHistoryDto> history, int totalCount) = await _pointsService.GetPointsHistoryAsync(
            userId,
            page,
            pageSize,
            context.CancellationToken);

        var response = new PointsHistoryResponse
        {
            UserId = request.UserId,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };

        response.Transactions.AddRange(history.Select(UserMapper.ToProtoPointsHistoryEntry));

        return response;
    }

    private string ParseOrThrowGuid(string guid)
    {
        return guid;
    }
}