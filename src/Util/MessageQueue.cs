using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SimStockMarket
{
    public class MessageQueue : IDisposable
    {
        private IModel _channel;
        private string _exchange;

        public MessageQueue(IModel channel, string exchange)
        {
            _channel = channel;
            _exchange = exchange;
        }

        public void Dispose()
        {
            if(_channel != null)
            {
                _channel.Dispose();
            }
        }

        public void Publish(string action, object message)
        {
            var body = JsonConvert.SerializeObject(message);

            _channel.BasicPublish(exchange: _exchange,
                                 routingKey: action,
                                 basicProperties: null,
                                 body: Encoding.UTF8.GetBytes(body));

            Debug.WriteLine($"***TX*** ({_exchange}:{action}) {body}");
        }

        public void Subscribe<TMessage>(Action<TMessage> handler)
        {
            var routingKey = typeof(TMessage).Name.ToLower();

            Debug.WriteLine($"Subscribing to queue {_exchange}:{routingKey}...");

            _channel.QueueDeclare(queue: _exchange,
                 durable: true,
                 exclusive: false,
                 autoDelete: false,
                 arguments: null);

            _channel.QueueBind(queue: _exchange,
                              exchange: _exchange,
                              routingKey: routingKey);

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, ea) => {
                try
                {
                    var body = Encoding.UTF8.GetString(ea.Body);

                    Debug.WriteLine($"***RX*** ({ea.Exchange}:{ea.RoutingKey}) {body}");

                    var message = JsonConvert.DeserializeObject<TMessage>(body);

                    handler(message);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error handling message:\r\n{ex}");
                }
            };

            _channel.BasicConsume(queue: _exchange,
                                noAck: true,
                                consumer: consumer);
        }

        public static MessageQueue Connect(IConnection connection, string exchange)
        {
            var channel = connection.CreateModel();

            Debug.WriteLine($"Connecting to exchange {exchange}...");

            channel.ExchangeDeclare(exchange: exchange, type: "direct");

            return new MessageQueue(channel, exchange);
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
