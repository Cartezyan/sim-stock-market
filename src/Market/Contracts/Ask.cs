using System;

namespace SimStockMarket.Market.Contracts
{
    public class Ask
    {
        internal DateTime Timestamp { get; set; }
        public string TraderId { get; set; }
        public string Symbol { get; set; }
        public decimal Price { get; set; }
    }
}
