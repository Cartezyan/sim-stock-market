using System;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace SimStockMarket.Extensions.RabbitMQ
{
    public interface IMessageQueue
    {
        void Publish(string action, object message);
        IMessageQueue Subscribe<TMessage>(Action<TMessage> handler);
    }

    public class MessageQueue : IMessageQueue
    {
        private IModel _channel;
        private string _exchange;

        public MessageQueue(IModel channel, string exchange)
        {
            _channel = channel;
            _exchange = exchange;
        }

        public void Declare()
        {
            Log.Verbose("Connecting to exchange {exchange}...", _exchange);
            _channel.ExchangeDeclare(exchange: _exchange, type: "direct");
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

        public IMessageQueue Subscribe<TMessage>(Action<TMessage> handler)
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

            return this;
        }
    }
}
