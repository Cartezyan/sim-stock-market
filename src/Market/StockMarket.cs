using System;
using System.Collections.Generic;
using System.Linq;
using SimStockMarket.Market.Contracts;

namespace SimStockMarket.Market
{
    public class StockMarket
    {
        private readonly ICollection<TradeOffer> Offers = new List<TradeOffer>();

        public Bid FindBuyer(Ask ask)
        {
            return GetOffersBySymbol(ask?.Symbol)
                .OfType<Bid>()
                .OrderBy(x => x.Price)
                .ThenBy(x => x.Timestamp)
                .FirstOrDefault(x => x.Price >= ask?.Price);
        }

        public Ask FindSeller(Bid bid)
        {
            return GetOffersBySymbol(bid?.Symbol)
                .OfType<Ask>()
                .OrderByDescending(x => x.Price)
                .ThenBy(x => x.Timestamp)
                .FirstOrDefault(x => x.Price <= bid?.Price);
        }

        public IEnumerable<TradeOffer> GetOffersBySymbol(string symbol)
        {
            return Offers.Where(x => string.Equals(x.Symbol, symbol, StringComparison.OrdinalIgnoreCase));
        }

        public void Resolve(TradeOffer offer)
        {
            var existing = GetExistingOffer(offer);

            if (existing != null)
                Offers.Remove(existing);
        }

        public void SubmitOffer(TradeOffer offer)
        {
            if (offer == null)
                throw new ArgumentNullException(nameof(offer));

            if (string.IsNullOrWhiteSpace(offer.Symbol))
                throw new ArgumentException("Invalid symbol");

            if (offer.Price <= 0)
                throw new ArgumentException("Offer price must be positive");

            offer.Timestamp = DateTime.UtcNow;

            var existing = GetExistingOffer(offer);

            if (existing != null)
            {
                Offers.Remove(existing);
            }

            Offers.Add(offer);
        }

        private TradeOffer GetExistingOffer(TradeOffer offer)
        {
            var existing = GetOffersBySymbol(offer.Symbol)
                            .FirstOrDefault(x => x.TraderId == offer.TraderId);
            return existing;
        }
    }
}
