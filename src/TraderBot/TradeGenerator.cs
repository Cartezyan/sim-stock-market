using System;

namespace SimStockMarket.TraderBot
{
    public enum TradeKind
    {
        Bid,
        Ask
    }

    public class Trade
    {
        public TradeKind Type { get; set; }
        public string TraderId { get; set; }
        public string Symbol { get; set; }
        public decimal Price { get; set; }
    }

    public class TradeGenerator
    {
        private static readonly string[] Symbols = new[] { "IBM", "MSFT", "GOOG" };
        private static readonly Random Random = new Random();

        private readonly string _traderId;

        public TradeGenerator(string traderId)
        {
            _traderId = traderId;
        }

        public Trade GenerateTrade()
        {
            var type = (Random.Next(2) == 0) ? TradeKind.Bid : TradeKind.Ask;
            var symbol = Symbols[Random.Next(0, Symbols.Length)];
            var price = Random.Next(1, 5);

            return new Trade {
                Type = type,
                TraderId = _traderId,
                Symbol = symbol,
                Price = price,
            };
        }
    }
}
