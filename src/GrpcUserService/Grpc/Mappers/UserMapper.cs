using DomainUserService.Dto;
using GrpcUserService.Grpc.Protos;
using PointsTransactionType = DomainUserService.Dto.PointsTransactionType;

namespace GrpcUserService.Grpc.Mappers;

public static class UserMapper
{
    public static DomainUserService.Models.UserRole ToDomainRole(UserRole protoRole)
    {
        return protoRole switch
        {
            UserRole.Guest => DomainUserService.Models.UserRole.GUEST,
            UserRole.Manager => DomainUserService.Models.UserRole.MANAGER,
            UserRole.Cashier => DomainUserService.Models.UserRole.CASHIER,
            UserRole.Hookahmaster => DomainUserService.Models.UserRole.HOOKAHMASTER,
            UserRole.Stockperson => DomainUserService.Models.UserRole.STOCKPERSON,
            UserRole.Administrator => DomainUserService.Models.UserRole.ADMINISTRATOR,
            UserRole.Cleaning => DomainUserService.Models.UserRole.CLEANING,
            UserRole.Unspecified => DomainUserService.Models.UserRole.GUEST,
            _ => DomainUserService.Models.UserRole.GUEST,
        };
    }

    public static Protos.UserRole ToProtoRole(DomainUserService.Models.UserRole domainRole)
    {
        return domainRole switch
        {
            DomainUserService.Models.UserRole.GUEST => UserRole.Guest,
            DomainUserService.Models.UserRole.MANAGER => UserRole.Manager,
            DomainUserService.Models.UserRole.CASHIER => UserRole.Cashier,
            DomainUserService.Models.UserRole.HOOKAHMASTER => UserRole.Hookahmaster,
            DomainUserService.Models.UserRole.STOCKPERSON => UserRole.Stockperson,
            DomainUserService.Models.UserRole.ADMINISTRATOR => UserRole.Administrator,
            DomainUserService.Models.UserRole.CLEANING => UserRole.Cleaning,
            _ => UserRole.Unspecified,
        };
    }

    public static CreateUserDto ToCreateUserDto(CreateUserRequest request)
    {
        Guid userId;
        if (!string.IsNullOrEmpty(request.Id) && Guid.TryParse(request.Id, out Guid parsedId))
        {
            userId = parsedId;
        }
        else
        {
            userId = Guid.NewGuid();
        }

        return new CreateUserDto(
            Id: userId,
            Username: request.Username,
            PasswordHash: BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role: ToDomainRole(request.Role));
    }

    public static UserDto ToUserDto(User protoUser)
    {
        return new UserDto(
            Id: Guid.Parse(protoUser.Id),
            Username: protoUser.Username,
            PhoneNumber: protoUser.PhoneNumber,
            PasswordHash: protoUser.PasswordHash,
            RegisteredAt: protoUser.RegisteredAt.ToDateTime(),
            IsBlocked: protoUser.IsBlocked,
            LoyaltyPoints: protoUser.LoyaltyPoints,
            Role: ToDomainRole(protoUser.Role));
    }

    public static User ToProtoUser(UserDto userDto)
    {
        var protoUser = new User
        {
            Id = userDto.Id.ToString(),
            Username = userDto.Username,
            PhoneNumber = userDto.PhoneNumber,
            RegisteredAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(userDto.RegisteredAt),
            IsBlocked = userDto.IsBlocked,
            LoyaltyPoints = userDto.LoyaltyPoints,
        };

        protoUser.Role = ToProtoRole(userDto.Role);

        return protoUser;
    }

    public static PointsHistoryEntry ToProtoPointsHistoryEntry(PointsHistoryDto dto)
    {
        var entry = new PointsHistoryEntry
        {
            Id = dto.Id.ToString(),
            UserId = dto.UserId.ToString(),
            Amount = Math.Abs(dto.Points),
            ReferenceId = dto.OrderId != Guid.Empty ? dto.OrderId.ToString() : null,
            CreatedAt = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(dto.CreatedAt),
        };

        entry.Type = dto.Type switch
        {
            PointsTransactionType.Add => GrpcUserService.Grpc.Protos.PointsTransactionType.Add,
            PointsTransactionType.Spend => GrpcUserService.Grpc.Protos.PointsTransactionType.Spend,
            PointsTransactionType.Compensate => GrpcUserService.Grpc.Protos.PointsTransactionType.Compensate,
            PointsTransactionType.Expire => GrpcUserService.Grpc.Protos.PointsTransactionType.Expire,
            _ => GrpcUserService.Grpc.Protos.PointsTransactionType.Unspecified,
        };

        return entry;
    }
}