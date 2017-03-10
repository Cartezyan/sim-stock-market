using System;
using MongoDB.Bson.Serialization.Attributes;

namespace SimStockMarket.Market.Contracts
{
    public enum TradeOfferKind
    {
        Bid,
        Ask,
    }

    [BsonKnownTypes(typeof(Ask), typeof(Bid))]
    public class TradeOffer
    {
        public MongoDB.Bson.ObjectId _id;
        internal DateTime Timestamp { get; set; }
        public string TraderId { get; set; }
        public string Symbol { get; set; }
        public decimal Price { get; set; }
    }
}
