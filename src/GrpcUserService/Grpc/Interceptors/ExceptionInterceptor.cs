using DomainUserService.Exceptions;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace GrpcUserService.Grpc.Interceptors;

public class ExceptionInterceptor : Interceptor
{
    private readonly ILogger<ExceptionInterceptor> _logger;

    public ExceptionInterceptor(ILogger<ExceptionInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (Exception ex)
        {
            return HandleException<TResponse>(ex, context);
        }
    }

    private TResponse HandleException<TResponse>(Exception ex, ServerCallContext context)
    {
        switch (ex)
        {
            case RpcException rpcEx:
                throw rpcEx;
            case CollisionException collisionEx:
                throw new RpcException(new Status(StatusCode.AlreadyExists, collisionEx.Message));
            case KeyNotFoundException keyNotFoundEx:
                throw new RpcException(new Status(StatusCode.NotFound, keyNotFoundEx.Message));
            default:
                _logger.LogError(ex, "Error processing gRPC call {Method}", context.Method);
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }
}