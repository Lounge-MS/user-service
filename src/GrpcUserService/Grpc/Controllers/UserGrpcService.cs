using DomainUserService.Dto;
using DomainUserService.Interfaces.IServices;
using Grpc.Core;
using GrpcUserService.Grpc.Mappers;
using GrpcUserService.Grpc.Protos;
using Microsoft.Extensions.Logging;

namespace GrpcUserService.Grpc.Controllers;

public class UserGrpcService : global::GrpcUserService.Grpc.Protos.UserService.UserServiceBase
{
    private readonly ILogger<UserGrpcService> _logger;
    private readonly IUserService _userService;
    private readonly IPointsService _pointsService;
    private readonly IEventPublisher _eventPublisher;

    public UserGrpcService(
        ILogger<UserGrpcService> logger,
        IUserService userService,
        IPointsService pointsService,
        IEventPublisher eventPublisher)
    {
        _logger = logger;
        _userService = userService;
        _pointsService = pointsService;
        _eventPublisher = eventPublisher;
    }

    public override async Task<UserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
    {
        try
        {
            var createUserDto = UserMapper.ToCreateUserDto(request);

            UserDto userDto = await _userService.CreateUserAsync(
                createUserDto,
                context.CancellationToken);

            await _eventPublisher.PublishUserRegisteredEvent(
                userDto.Id.ToString());

            return new UserResponse
            {
                Success = true,
                User = UserMapper.ToProtoUser(userDto),
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return new UserResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred while creating user",
                    Details = { { "exception", ex.Message } },
                },
            };
        }
    }

    public override async Task<UserResponse> GetUser(GetUserRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.UserId, out Guid userId))
            {
                return new UserResponse
                {
                    Success = false,
                    Error = new Error
                    {
                        Code = "INVALID_INPUT",
                        Message = "Invalid user ID format",
                    },
                };
            }

            UserDto userDto = await _userService.GetUserAsync(
                userId,
                context.CancellationToken);

            return new UserResponse
            {
                Success = true,
                User = UserMapper.ToProtoUser(userDto),
            };
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "User not found: {UserId}", request.UserId);
            return new UserResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "USER_NOT_FOUND",
                    Message = $"User with ID {request.UserId} not found",
                },
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", request.UserId);
            return new UserResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred while getting user",
                },
            };
        }
    }

    public override async Task<UserResponse> UpdateUser(UpdateUserRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.UserId, out Guid userId))
            {
                return new UserResponse
                {
                    Success = false,
                    Error = new Error
                    {
                        Code = "INVALID_INPUT",
                        Message = "Invalid user ID format",
                    },
                };
            }

            UserDto existingUser = await _userService.GetUserAsync(
                userId,
                context.CancellationToken);

            var updateDto = new UserDto(
                userId,
                request.Username ?? existingUser.Username,
                request.PhoneNumber ?? existingUser.PhoneNumber,
                request.Password,
                existingUser.RegisteredAt,
                existingUser.IsBlocked,
                existingUser.LoyaltyPoints,
                existingUser.Role);

            UserDto userDto = await _userService.UpdateUserAsync(
                updateDto,
                context.CancellationToken);

            return new UserResponse
            {
                Success = true,
                User = UserMapper.ToProtoUser(userDto),
            };
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "User not found: {UserId}", request.UserId);
            return new UserResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "USER_NOT_FOUND",
                    Message = $"User with ID {request.UserId} not found",
                },
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", request.UserId);
            return new UserResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred while updating user",
                },
            };
        }
    }

    public override async Task<UserResponse> UpdateUserRole(UpdateUserRoleRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.UserId, out Guid userId))
            {
                return new UserResponse
                {
                    Success = false,
                    Error = new Error
                    {
                        Code = "INVALID_INPUT",
                        Message = "Invalid user ID format",
                    },
                };
            }

            UserDto userDto = await _userService.UpdateUserRoleAsync(
                userId,
                UserMapper.ToDomainRole(request.Role),
                context.CancellationToken);

            return new UserResponse
            {
                Success = true,
                User = UserMapper.ToProtoUser(userDto),
            };
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "User not found: {UserId}", request.UserId);
            return new UserResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "USER_NOT_FOUND",
                    Message = $"User with ID {request.UserId} not found",
                },
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user role {UserId}", request.UserId);
            return new UserResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred while updating user role",
                },
            };
        }
    }

    public override async Task<UserResponse> BlockUser(BlockUserRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.UserId, out Guid userId))
            {
                return new UserResponse
                {
                    Success = false,
                    Error = new Error
                    {
                        Code = "INVALID_INPUT",
                        Message = "Invalid user ID format",
                    },
                };
            }

            UserDto userDto = await _userService.BlockUserAsync(
                userId,
                context.CancellationToken);

            return new UserResponse
            {
                Success = true,
                User = UserMapper.ToProtoUser(userDto),
            };
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "User not found: {UserId}", request.UserId);
            return new UserResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "USER_NOT_FOUND",
                    Message = $"User with ID {request.UserId} not found",
                },
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error blocking user {UserId}", request.UserId);
            return new UserResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred while blocking user",
                },
            };
        }
    }

    public override async Task<UserResponse> UnblockUser(UnblockUserRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.UserId, out Guid userId))
            {
                return new UserResponse
                {
                    Success = false,
                    Error = new Error
                    {
                        Code = "INVALID_INPUT",
                        Message = "Invalid user ID format",
                    },
                };
            }

            UserDto userDto = await _userService.UnblockUserAsync(
                userId,
                context.CancellationToken);

            return new UserResponse
            {
                Success = true,
                User = UserMapper.ToProtoUser(userDto),
            };
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "User not found: {UserId}", request.UserId);
            return new UserResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "USER_NOT_FOUND",
                    Message = $"User with ID {request.UserId} not found",
                },
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unblocking user {UserId}", request.UserId);
            return new UserResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred while unblocking user",
                },
            };
        }
    }

    public override async Task<UserResponse> DeleteUser(DeleteUserRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.UserId, out Guid userId))
            {
                return new UserResponse
                {
                    Success = false,
                    Error = new Error
                    {
                        Code = "INVALID_INPUT",
                        Message = "Invalid user ID format",
                    },
                };
            }

            UserDto userDto = await _userService.DeleteUserAsync(
                userId,
                context.CancellationToken);

            await _eventPublisher.PublishUserDeletedEvent(
                request.UserId);

            return new UserResponse
            {
                Success = true,
                User = UserMapper.ToProtoUser(userDto),
            };
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "User not found: {UserId}", request.UserId);
            return new UserResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "USER_NOT_FOUND",
                    Message = $"User with ID {request.UserId} not found",
                },
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", request.UserId);
            return new UserResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred while deleting user",
                },
            };
        }
    }

    public override async Task<ListUsersResponse> ListUsers(ListUsersRequest request, ServerCallContext context)
    {
        try
        {
            (List<UserDto> users, int totalCount) = await _userService.ListUsersAsync(
                request.Query,
                request.Page,
                request.PageSize,
                context.CancellationToken);

            var response = new ListUsersResponse
            {
                Success = true,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
            };

            response.Users.AddRange(users.Select(UserMapper.ToProtoUser));

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing users");
            return new ListUsersResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred while listing users",
                },
            };
        }
    }

    public override async Task<PointsResponse> GetUserPoints(GetUserPointsRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.UserId, out Guid userId))
            {
                return new PointsResponse
                {
                    Success = false,
                    Error = new Error
                    {
                        Code = "INVALID_INPUT",
                        Message = "Invalid user ID format",
                    },
                };
            }

            int points = await _pointsService.GetUserPointsAsync(
                userId,
                context.CancellationToken);

            return new PointsResponse
            {
                Success = true,
                UserId = request.UserId,
                Points = points,
            };
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "User not found: {UserId}", request.UserId);
            return new PointsResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "USER_NOT_FOUND",
                    Message = $"User with ID {request.UserId} not found",
                },
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user points {UserId}", request.UserId);
            return new PointsResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred while getting user points",
                },
            };
        }
    }

    public override async Task<SpendPointsResponse> SpendPoints(SpendPointsRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.UserId, out Guid userId))
            {
                return new SpendPointsResponse
                {
                    Success = false,
                    UserId = request.UserId,
                    Error = new Error
                    {
                        Code = "INVALID_INPUT",
                        Message = "Invalid user ID format",
                    },
                };
            }

            (bool Success, int NewBalance, string? Error) result = await _pointsService.SpendPointsAsync(
                userId,
                request.Amount,
                request.ReferenceId,
                context.CancellationToken);

            if (!result.Success)
            {
                return new SpendPointsResponse
                {
                    Success = false,
                    UserId = request.UserId,
                    Error = new Error
                    {
                        Code = "POINTS_SPEND_FAILED",
                        Message = result.Error,
                    },
                };
            }

            await _eventPublisher.PublishPointsSpentEvent(
                request.UserId,
                request.Amount,
                request.ReferenceId);

            return new SpendPointsResponse
            {
                Success = true,
                UserId = request.UserId,
                NewBalance = result.NewBalance,
            };
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "User not found: {UserId}", request.UserId);
            return new SpendPointsResponse
            {
                Success = false,
                UserId = request.UserId,
                Error = new Error
                {
                    Code = "USER_NOT_FOUND",
                    Message = $"User with ID {request.UserId} not found",
                },
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error spending points for user {UserId}", request.UserId);
            return new SpendPointsResponse
            {
                Success = false,
                UserId = request.UserId,
                Error = new Error
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred while spending points",
                },
            };
        }
    }

    public override async Task<PointsResponse> AddPoints(AddPointsRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.UserId, out Guid userId))
            {
                return new PointsResponse
                {
                    Success = false,
                    Error = new Error
                    {
                        Code = "INVALID_INPUT",
                        Message = "Invalid user ID format",
                    },
                };
            }

            if (request.Amount <= 0)
            {
                return new PointsResponse
                {
                    Success = false,
                    Error = new Error
                    {
                        Code = "INVALID_INPUT",
                        Message = "Amount must be positive",
                    },
                };
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
                Success = true,
                UserId = request.UserId,
                Points = newBalance,
            };
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "User not found: {UserId}", request.UserId);
            return new PointsResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "USER_NOT_FOUND",
                    Message = $"User with ID {request.UserId} not found",
                },
            };
        }
        catch (ArgumentOutOfRangeException ex)
        {
            _logger.LogWarning(ex, "Invalid amount: {Amount}", request.Amount);
            return new PointsResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "INVALID_INPUT",
                    Message = ex.Message,
                },
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding points for user {UserId}", request.UserId);
            return new PointsResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred while adding points",
                },
            };
        }
    }

    public override async Task<CompensatePointsResponse> CompensatePoints(
        CompensatePointsRequest request,
        ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.UserId, out Guid userId))
            {
                return new CompensatePointsResponse
                {
                    Success = false,
                    Error = new Error
                    {
                        Code = "INVALID_INPUT",
                        Message = "Invalid user ID format",
                    },
                };
            }

            if (request.Amount <= 0)
            {
                return new CompensatePointsResponse
                {
                    Success = false,
                    Error = new Error
                    {
                        Code = "INVALID_INPUT",
                        Message = "Amount must be positive",
                    },
                };
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
                Success = result.Success,
                UserId = request.UserId,
                NewBalance = result.NewBalance,
            };
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "User not found: {UserId}", request.UserId);
            return new CompensatePointsResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "USER_NOT_FOUND",
                    Message = $"User with ID {request.UserId} not found",
                },
            };
        }
        catch (ArgumentOutOfRangeException ex)
        {
            _logger.LogWarning(ex, "Invalid amount: {Amount}", request.Amount);
            return new CompensatePointsResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "INVALID_INPUT",
                    Message = ex.Message,
                },
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error compensating points for user {UserId}", request.UserId);
            return new CompensatePointsResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred while compensating points",
                },
            };
        }
    }

    public override async Task<PointsHistoryResponse> GetPointsHistory(
        GetPointsHistoryRequest request,
        ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.UserId, out Guid userId))
            {
                return new PointsHistoryResponse
                {
                    Success = false,
                    Error = new Error
                    {
                        Code = "INVALID_INPUT",
                        Message = "Invalid user ID format",
                    },
                };
            }

            int page = request.Page > 0 ? request.Page : 1;
            int pageSize = request.PageSize > 0 ? request.PageSize : 10;

            (List<PointsHistoryDto> history, int totalCount) = await _pointsService.GetPointsHistoryAsync(
                userId,
                page,
                pageSize,
                context.CancellationToken);

            var response = new PointsHistoryResponse
            {
                Success = true,
                UserId = request.UserId,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
            };

            response.Transactions.AddRange(history.Select(UserMapper.ToProtoPointsHistoryEntry));

            return response;
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "User not found: {UserId}", request.UserId);
            return new PointsHistoryResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "USER_NOT_FOUND",
                    Message = $"User with ID {request.UserId} not found",
                },
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting points history for user {UserId}", request.UserId);
            return new PointsHistoryResponse
            {
                Success = false,
                Error = new Error
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An error occurred while getting points history",
                },
            };
        }
    }
}