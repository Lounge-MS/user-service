using KafkaUserService.Configuration;
using KafkaUserService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KafkaUserService.Extensions;

public static class KafkaServiceCollectionExtensions
{
    public static IServiceCollection AddKafkaEventPublisher(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        IConfigurationSection section = configuration.GetSection(KafkaOptions.SectionName);
        services.Configure<KafkaOptions>(section);

        services.AddSingleton<KafkaEventPublisher>();
        services.AddSingleton<DomainUserService.Interfaces.IServices.IEventPublisher>(
            sp => sp.GetRequiredService<KafkaEventPublisher>());

        return services;
    }
}
