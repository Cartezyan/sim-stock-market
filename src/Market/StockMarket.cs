using System;
using System.Collections.Generic;
using System.Linq;
using SimStockMarket.Market.Contracts;

namespace SimStockMarket.Market
{
    public class StockMarket
    {
        private readonly ICollection<Ask> Asks = new List<Ask>();
        private readonly ICollection<Bid> Bids = new List<Bid>();

        public Bid FindBuyer(Ask ask)
        {
            return GetBidsBySymbol(ask?.Symbol)
                .OrderBy(x => x.Price)
                .ThenBy(x => x.Timestamp)
                .FirstOrDefault(x => x.Price >= ask?.Price);
        }

        public Ask FindSeller(Bid bid)
        {
            return GetAsksBySymbol(bid?.Symbol)
                .OrderByDescending(x => x.Price)
                .ThenBy(x => x.Timestamp)
                .FirstOrDefault(x => x.Price <= bid?.Price);
        }

        public IEnumerable<Ask> GetAsksBySymbol(string symbol)
        {
            return Asks.Where(x => string.Equals(x.Symbol, symbol, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<Bid> GetBidsBySymbol(string symbol)
        {
            return Bids.Where(x => string.Equals(x.Symbol, symbol, StringComparison.OrdinalIgnoreCase));
        }

        public void ResolveAsk(Ask ask, Guid tradeId)
        {
            GetAsksBySymbol(ask.Symbol)
                .Where(x => x.Price == ask.Price && x.TraderId == ask.TraderId)
                .ToList()
                .ForEach(x => Asks.Remove(x));
        }

        public void ResolveBid(Bid bid, Guid tradeId)
        {
            GetBidsBySymbol(bid?.Symbol)
                .Where(x => x.Price == bid.Price && x.TraderId == bid.TraderId)
                .ToList()
                .ForEach(x => Bids.Remove(x));
        }

        public void SubmitBid(Bid bid)
        {
            if (bid == null)
                throw new ArgumentNullException(nameof(bid));

            if (string.IsNullOrWhiteSpace(bid.Symbol))
                throw new ArgumentException("Invalid symbol");

            if (bid.Price <= 0)
                throw new ArgumentException("Bid price must be positive");

            bid.Timestamp = DateTime.UtcNow;

            Bids.Add(bid);
        }

        public void SubmitAsk(Ask ask)
        {
            if (ask == null)
                throw new ArgumentNullException(nameof(ask));

            if (string.IsNullOrWhiteSpace(ask.Symbol))
                throw new ArgumentException("Invalid symbol");

            if (ask.Price <= 0)
                throw new ArgumentException("Ask price must be positive");

            ask.Timestamp = DateTime.UtcNow;

            Asks.Add(ask);
        }
    }
}
