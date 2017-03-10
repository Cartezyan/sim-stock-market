using System;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

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
                if(_channel.IsOpen)
                    _channel.Close();

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

            Log.Verbose("***TX*** ({exchange}:{action}) {body}", _exchange, action, body);
        }

        public void Subscribe<TMessage>(Action<TMessage> handler)
        {
            var routingKey = typeof(TMessage).Name.ToLower();

            Log.Debug("Subscribing to queue {exchange}:{routingKey}...", _exchange, routingKey);

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

                    Log.Verbose("***RX*** ({exchange}:{routingKey}) {body}", 
                                ea.Exchange, ea.RoutingKey, body);

                    var message = JsonConvert.DeserializeObject<TMessage>(body);

                    handler(message);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error handling message");
                }
            };

            _channel.BasicConsume(queue: _exchange,
                                noAck: true,
                                consumer: consumer);
        }

        public static MessageQueue Connect(IConnection connection, string exchange)
        {
            var channel = connection.CreateModel();

            Log.Verbose("Connecting to exchange {exchange}...", exchange);

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
                    var connection = factory.CreateConnection();
                    Log.Information("Connected to {host}", host);
                    return connection;
                }
                catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException)
                {
                    Log.Warning("Failed to connect; retrying in {delay} seconds...", connectionDelay.TotalSeconds);
                    Thread.Sleep(connectionDelay);
                    connectionDelay += connectionDelay;
                }
            }
        }
    }
}
