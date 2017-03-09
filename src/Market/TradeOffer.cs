using System;

namespace SimStockMarket.Market
{
    public enum TradeOfferKind
    {
        Bid,
        Ask,
    }

    public abstract class TradeOffer
    {
        internal DateTime Timestamp { get; set; }
        public string TraderId { get; set; }
        public string Symbol { get; set; }
        public decimal Price { get; set; }
    }
}
