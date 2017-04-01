using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace SimStockMarket.Extensions.Redis
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddRedis(this IServiceCollection services, string redisHost)
        {
            return services
                .AddSingleton<ConnectionMultiplexer>(_ => ConnectToRedis(redisHost))
                .AddSingleton<IDatabase>(GetRedisDatabase)
                .AddSingleton<IMessageBus, MessageBus>();
        }

        static ConnectionMultiplexer ConnectToRedis(string host)
        {
            var hostIps = Dns.GetHostAddressesAsync(host).Result;
            var ipv4Ips = hostIps.Where(x => x.AddressFamily == AddressFamily.InterNetwork);
            var hostIp = ipv4Ips.Select(x => x.ToString()).FirstOrDefault();
            return ConnectionMultiplexer.Connect(hostIp);
        }

        static IDatabase GetRedisDatabase(IServiceProvider services)
        {
            var redis = services.GetService<ConnectionMultiplexer>();
            return redis.GetDatabase();
        }
    }
}
