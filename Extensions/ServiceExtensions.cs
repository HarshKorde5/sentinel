using Microsoft.EntityFrameworkCore;
using Sentinel.Common;
using Sentinel.Data;
using Sentinel.Services;

namespace Sentinel.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddSentinelServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        
        services.Configure<OllamaOptions>(
            config.GetSection(OllamaOptions.Section));

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("Default")));
            
        services.AddHttpClient<IOllamaService, OllamaService>();

        return services;
    }
}