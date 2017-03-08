using System;
using RabbitMQ.Client;
using System.Text;
using RabbitMQ.Client.Events;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;
using SimStockMarket.Market.Handlers;
using Microsoft.Extensions.DependencyInjection;
using SimStockMarket.Market.Contracts;
using System.Linq;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace SimStockMarket.Market
{
    class Program
    {
        static readonly IDictionary<Type, Type> actionHandlers = new Dictionary<Type, Type>
            {
                { typeof(Ask), typeof(AskHandler) },
                { typeof(Bid), typeof(BidHandler) },
            };
        static readonly IServiceProvider Services;

        static readonly IConfiguration Config;

        static Program()
        {
            Config = new ConfigurationBuilder()
                        .AddInMemoryCollection(new Dictionary<string,string>
                        {
                            { "QueueHostName", "localhost" },
                            { "QueueName", "stockmarket" },
                        })
                        .AddEnvironmentVariables()
                        .Build();

            Services = new ServiceCollection()
                .AddSingleton<StockMarket>()
                .AddSingleton<TradeLedger>()
                .AddTransient<AskHandler>()
                .AddTransient<BidHandler>()
                .AddTransient<TradeRequestHandler>()
                .BuildServiceProvider();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("==== Stock Market ====");

            var queueHost = Config["QueueHostName"];
            var queueName = Config["QueueName"];

            Debug.WriteLine($"Connecting to queue host {queueHost}...");

            using (var connection = Connect(queueHost))
            using (var channel = connection.CreateModel())
            {
                Debug.WriteLine($"Subscribing to queue {queueName}...");

                channel.QueueDeclare(queue: queueName,
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += (model, ea) =>
                {
                    var body = Encoding.UTF8.GetString(ea.Body);

                    Debug.WriteLine($"***RX*** {body}");

                    var splitIndex = body.IndexOf(':');
                    var action = body.Substring(0, splitIndex);
                    var message = body.Substring(splitIndex + 1);

                    Handle(action, message);
                };

                channel.BasicConsume(queue: queueName,
                                     noAck: true,
                                     consumer: consumer);

                Console.WriteLine("Market started");
                
                while(true)
                {
                    Thread.Sleep(100);
                }
            }
        }

        private static void Handle(string action, string body)
        {
            try
            {
                var type = actionHandlers.Keys.FirstOrDefault(x => string.Equals(x.Name, action, StringComparison.OrdinalIgnoreCase));
                var handlerType = actionHandlers[type];
                var handler = (IHandler)Services.GetService(handlerType);

                var message = JsonConvert.DeserializeObject(body, type);

                handler.Handle(message);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to handle message:\r\n{ex}");
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