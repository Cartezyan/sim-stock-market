using Newtonsoft.Json;
using StackExchange.Redis;

namespace SimStockMarket
{
    public interface IMessageBus
    {
        void Publish(string channel, string message);
        void Publish<TMessage>(string channel, TMessage message) where TMessage : class;
    }

    public class MessageBus : IMessageBus
    {
        private readonly IDatabase _redis;

        public MessageBus(IDatabase redis)
        {
            _redis = redis;
        }

        public void Publish(string channel, string message)
        {
            _redis.Publish(channel, message);
        }

        public void Publish<TMessage>(TMessage message) where TMessage : class
        {
            var channel = typeof(TMessage).Name.ToLower();
            Publish(channel, message);
        }

        public void Publish<TMessage>(string channel, TMessage message) where TMessage : class
        {
            var serialized = JsonConvert.SerializeObject(message);
            Publish(channel, serialized);
        }
    }
}
