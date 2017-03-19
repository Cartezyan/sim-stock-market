using System;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace SimStockMarket.Market
{
    public enum TradeOfferKind
    {
        Bid,
        Ask,
    }

    [BsonKnownTypes(typeof(Ask), typeof(Bid))]
    public class TradeOffer
    {
        [JsonIgnore]
        public MongoDB.Bson.ObjectId _id;

        public DateTime Timestamp { get; set; }
        public string TraderId { get; set; }
        public string Symbol { get; set; }
        public decimal Price { get; set; }
    }
}
