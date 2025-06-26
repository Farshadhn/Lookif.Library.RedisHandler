using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis; 

namespace Lookif.Library.RedisHandler
{
    public static class RedisServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisHandler(this IServiceCollection services, Action<RedisOptions> configureOptions)
        {
            var options = new RedisOptions();
            configureOptions(options);
            if (string.IsNullOrWhiteSpace(options.Configuration))
                throw new ArgumentException("Redis configuration string must be provided.", nameof(options.Configuration));

            services.AddSingleton(options);

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var configOptions = ConfigurationOptions.Parse(options.Configuration);
                if (!string.IsNullOrWhiteSpace(options.Username))
                    configOptions.User = options.Username;
                if (!string.IsNullOrWhiteSpace(options.Password))
                    configOptions.Password = options.Password;
                configOptions.Ssl = options.SSL;
                return ConnectionMultiplexer.Connect(configOptions);
            });

            services.AddSingleton<RedisHandlerService>(sp =>
            {
                var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
                var opts = sp.GetRequiredService<RedisOptions>();
                return new RedisHandlerService(multiplexer, opts.Database);
            });

            // Add more Redis-related services here if needed

            return services;
        }
    }
} 