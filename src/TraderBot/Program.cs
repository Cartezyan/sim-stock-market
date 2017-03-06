using System;
using RabbitMQ.Client;
using System.Text;
using System.Threading;

namespace SimStockMarket.TraderBot
{
    class Program
    {
        static readonly string QueueHostName = Environment.GetEnvironmentVariable("QueueHostName") ?? "localhost";
        static readonly string QueueName = Environment.GetEnvironmentVariable("QueueName") ?? "stockmarket";

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Trade Bot...");

            Console.WriteLine($"Connecting to message host {QueueHostName}...");

            using (var connection = Connect(QueueHostName))
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: QueueName,
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

                int tradeCounter = 0;

                while (true)
                {
                    var message = $"== New Trade {++tradeCounter} == ";

                    channel.BasicPublish(exchange: "",
                                         routingKey: QueueName,
                                         basicProperties: null,
                                         body: Encoding.UTF8.GetBytes(message));

                    Console.WriteLine($" [{QueueName}] Sent: {message}...");

                    Thread.Sleep(1000);
                }
            }
        }

        static IConnection Connect(string host)
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
                    return factory.CreateConnection();
                }
                catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException ex)
                {
                    Console.WriteLine($"Failed to connect: {ex.Message}");
                    Console.WriteLine($"Waiting {connectionDelay.TotalSeconds} seconds to try again...");
                    Thread.Sleep(connectionDelay);
                    connectionDelay += connectionDelay;
                }
            }
        }
    }
}