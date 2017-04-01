using System;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace SimStockMarket.Extensions.RabbitMQ
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddRabbitMQ(this IServiceCollection services, string host)
        {
            return services
                .AddSingleton<IConnection>(_ => MessageQueueFactory.Connect(host))
                .AddSingleton<IModel>(CreateChannel);
        }

        public static IServiceCollection AddMessageQueue(this IServiceCollection services, string host, string queueName)
        {
            return services
                .AddRabbitMQ(host)
                .AddSingleton<IMessageQueue>(_ => CreateMessageQueue(_, queueName));
        }

        static IModel CreateChannel(IServiceProvider services)
        {
            var connection = services.GetService<IConnection>();
            return connection.CreateModel();
        }

        static IMessageQueue CreateMessageQueue(IServiceProvider services, string queueName)
        {
            var model = services.GetService<IModel>();
            return MessageQueueFactory.CreateMessageQueue(model, queueName);
        }
    }
}
