using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Serilog;
using System.Threading;

namespace SimStockMarket.Extensions.RabbitMQ
{
    public static class MessageQueueFactory
    {
        public static IMessageQueue CreateMessageQueue(IModel channel, string queueName)
        {
            var queue = new MessageQueue(channel, queueName);
            queue.Declare();
            return queue;
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
                    if (connectionDelay.TotalSeconds > 30)
                        Log.Warning("Failed to connect; retrying in {delay} seconds...", connectionDelay.TotalSeconds);

                    Thread.Sleep(connectionDelay);
                    connectionDelay += connectionDelay;
                }
            }
        }
    }
}
