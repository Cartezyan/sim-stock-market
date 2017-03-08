using Newtonsoft.Json;
using System;

namespace SimStockMarket.TraderBot
{
    public class TradeGenerator
    {
        private static readonly string[] Symbols = new[] { "IBM", "MSFT", "GOOG" };
        private static readonly Random Random = new Random();

        private readonly string _traderId;

        public TradeGenerator(string traderId)
        {
            _traderId = traderId;
        }

        public string GenerateMessage()
        {
            var action = (Random.Next(2) == 0) ? "BID" : "ASK";
            var trade = GenerateTrade();

            return $"{action}:{JsonConvert.SerializeObject(trade)}";
        }

        private dynamic GenerateTrade()
        {
            var symbol = Symbols[Random.Next(0, Symbols.Length)];

            return new {
                TraderId = _traderId,
                Symbol = symbol,
                Price = Random.Next(1, 5),
            };
        }
    }
}
