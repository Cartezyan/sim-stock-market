using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Serilog;

namespace SimStockMarket.Extensions.RabbitMQ
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddRabbitMQ(this IServiceCollection services, string host)
        {
            return services
                .AddSingleton<IConnection>(_ => Connect(host))
                .AddSingleton<IModel>(CreateChannel);
        }

        public static IServiceCollection AddMessageQueue(this IServiceCollection services, string host, string queueName)
        {
            return services
                .AddRabbitMQ(host)
                .AddSingleton<IMessageQueue>(_ => CreateMessageQueue(_, queueName));
        }

        private static IMessageQueue CreateMessageQueue(IServiceProvider services, string queueName)
        {
            var channel = services.GetService<IModel>();
            var queue = new MessageQueue(channel, queueName);
            queue.Declare();
            return queue;
        }

        private static IModel CreateChannel(IServiceProvider services)
        {
            var connection = services.GetService<IConnection>();
            return connection.CreateModel();
        }

        public static IConnection Connect(string host)
        {
            var factory = new ConnectionFactory()
            {
                AutomaticRecoveryEnabled = true,
                HostName = host
            };

            var connectionDelay = TimeSpan.FromSeconds(1);

            while (true)
            {
                try
                {
                    var connection = factory.CreateConnection();
                    Log.Information("Connected to {host}", host);
                    return connection;
                }
                catch (BrokerUnreachableException)
                {
                    if(connectionDelay.TotalSeconds > 30)
                        Log.Warning("Failed to connect; retrying in {delay} seconds...", connectionDelay.TotalSeconds);

                    Thread.Sleep(connectionDelay);
                    connectionDelay += connectionDelay;
                }
            }
        }
    }
}
