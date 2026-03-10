using Microsoft.EntityFrameworkCore;
using Sentinel.Common;
using Sentinel.Data;
using Sentinel.Services;
using StackExchange.Redis;

namespace Sentinel.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddSentinelServices(
        this IServiceCollection services,
        IConfiguration config)
    {

        //options
        
        services.Configure<OllamaOptions>(config.GetSection(OllamaOptions.Section));

        services.Configure<RedisOptions>(config.GetSection(RedisOptions.Section));

        services.Configure<CacheOptions>(config.GetSection(CacheOptions.Section));



        //postgresql
        services.AddDbContext<AppDbContext>(options =>options.UseNpgsql(config.GetConnectionString("Default")));


        //redis : Singleton because one connection will be shared across all requests
        var redisConn = config["Redis:ConnectionString"] ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConn));
            

        //Application services
        services.AddHttpClient<IOllamaService, OllamaService>();

        //scoped means new instance per request because AppDbContext is scoped
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<IRateLimiter, RateLimiter>();

        return services;
    }
}