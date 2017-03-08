using System;
using RabbitMQ.Client;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace SimStockMarket.TraderBot
{
    class Program
    {
        static readonly IConfiguration Config;

        static Program()
        {
            Config = new ConfigurationBuilder()
                        .AddInMemoryCollection(new Dictionary<string, string>
                        {
                            { "QueueHostName", "localhost" },
                            { "QueueName", "stockmarket" },
                        })
                        .AddEnvironmentVariables()
                        .Build();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Trade Bot...");

            var queueHost = Config["QueueHostName"];
            var queueName = Config["QueueName"];

            Debug.WriteLine($"Connecting to message host {queueHost}...");

            using (var connection = Connect(queueHost))
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: queueName,
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

                var trader = new TradeGenerator(Environment.MachineName);

                while (true)
                {
                    var message = trader.GenerateMessage();

                    channel.BasicPublish(exchange: "",
                                         routingKey: queueName,
                                         basicProperties: null,
                                         body: Encoding.UTF8.GetBytes(message));

                    Debug.WriteLine($" [{queueName}] Sent: {message}...");

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