using Microsoft.Extensions.DependencyInjection;

namespace GrpcUserService.Grpc.Extensions;

public static class GrpcServiceCollectionExtensions
{
    public static IServiceCollection AddGrpcServices(this IServiceCollection services)
    {
        services.AddGrpc(options =>
        {
            options.EnableDetailedErrors = true;
            options.MaxReceiveMessageSize = 4 * 1024 * 1024;
            options.MaxSendMessageSize = 4 * 1024 * 1024;
        });

        services.AddGrpc();

        return services;
    }
}
