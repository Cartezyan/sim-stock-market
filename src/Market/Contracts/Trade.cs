using System;

namespace SimStockMarket.Market.Contracts
{
    public class Trade
    {
        public Guid Id { get; private set; }
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
            Id = Guid.NewGuid();
        }
    }
}
