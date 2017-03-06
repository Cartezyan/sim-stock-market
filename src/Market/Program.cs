using System;
using RabbitMQ.Client;
using System.Text;
using RabbitMQ.Client.Events;
using System.Threading;

namespace SimStockMarket.Market
{
    class Program
    {
        static readonly string QueueHostName = Environment.GetEnvironmentVariable("QueueHostName") ?? "localhost";
        static readonly string QueueName = Environment.GetEnvironmentVariable("QueueName") ?? "stockmarket";

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Market...");

            var factory = new ConnectionFactory() {
                AutomaticRecoveryEnabled = true,
                HostName = QueueHostName
            };

            Console.WriteLine($"Connecting to message host {QueueHostName}...");

            using (var connection = Connect(QueueHostName))
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: QueueName,
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

                Console.WriteLine($"Subscribing to queue {QueueName}...");

                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine($" [{QueueName}] Received: {message}");
                };

                channel.BasicConsume(queue: QueueName,
                                     noAck: true,
                                     consumer: consumer);

                Console.WriteLine("Market started");
                
                while(true)
                {
                    Thread.Sleep(100);
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