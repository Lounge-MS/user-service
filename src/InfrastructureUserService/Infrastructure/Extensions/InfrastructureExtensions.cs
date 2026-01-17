using DomainUserService.Domain.Services;
using DomainUserService.Interfaces.IRepositories;
using DomainUserService.Interfaces.IServices;
using FluentMigrator.Runner;
using InfrastructureUserService.Infrastructure.Migrations;
using InfrastructureUserService.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace InfrastructureUserService.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IUserRepository>(sp =>
            new UserRepository(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IPointsHistoryRepository>(sp =>
            new PointsHistoryRepository(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPointsService, PointsService>();

        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddPostgres()
                .WithGlobalConnectionString(configuration.GetConnectionString("DefaultConnection"))
                .ScanIn(typeof(InitialMigration).Assembly)
                .For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole());

        return services;
    }

    public static IServiceProvider UseInfrastructureMigrations(this IServiceProvider serviceProvider)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

        runner.MigrateUp();

        return serviceProvider;
    }
}