using Newtonsoft.Json;
using System;

namespace SimStockMarket.Market
{
    public class Trade
    {
        [JsonIgnore]
        public MongoDB.Bson.ObjectId _id;

        public DateTime Timestamp { get; private set; }
        public string SellerId { get; private set; }
        public string BuyerId { get; private set; }
        public string Symbol { get; private set; }
        public decimal Price { get; private set; }

        public Trade(string symbol, decimal price, string sellerId, string buyerId)
        {
            Symbol = symbol;
            Price = price;
            SellerId = sellerId;
            BuyerId = buyerId;
            Timestamp = DateTime.UtcNow;
        }
    }
}
