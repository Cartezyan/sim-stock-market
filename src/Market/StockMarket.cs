using System;
using System.Collections.Generic;
using System.Linq;
using SimStockMarket.Market.Contracts;
using MongoDB.Driver;
using MongoDB.Bson;
using Serilog;

namespace SimStockMarket.Market
{
    public class StockMarket
    {
        private static ILogger Log = Serilog.Log.ForContext<StockMarket>();

        private readonly IMongoDatabase _db;

        private IMongoCollection<TradeOffer> _offers;

        protected IMongoCollection<TradeOffer> Offers
        {
            get
            {
                _offers = (_offers == null) ? _db.GetCollection<TradeOffer>("offers") : _offers;
                return _offers;
            }
        }

        public StockMarket(IMongoDatabase db)
        {
            _db = db;
        }

        public Bid FindBuyer(Ask ask)
        {
            Log.Verbose("Finding buyer for {@ask}...", ask);

            return GetOffersBySymbol(ask?.Symbol)
                .OfType<Bid>()
                .OrderBy(x => x.Price)
                .ThenBy(x => x.Timestamp)
                .FirstOrDefault(x => x.Price >= ask?.Price);
        }

        public Ask FindSeller(Bid bid)
        {
            Log.Verbose("Finding seller for {@bid}...", bid);

            return GetOffersBySymbol(bid?.Symbol)
                .OfType<Ask>()
                .OrderByDescending(x => x.Price)
                .ThenBy(x => x.Timestamp)
                .FirstOrDefault(x => x.Price <= bid?.Price);
        }

        public IEnumerable<TradeOffer> GetOffersBySymbol(string symbol)
        {
            return Offers.AsQueryable().Where(x => x.Symbol == symbol);
        }

        public void Resolve(TradeOffer offer)
        {
            Log.Verbose("Resolving {@offer}...", offer);

            Offers.DeleteOne(x =>
                  x.TraderId == offer.TraderId
                && x.Symbol == offer.Symbol
            );

            Log.Debug("Resolved {@offer}", offer);
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

            // If I knew how to actually use MongoDB, I could get rid of this conditional
            if (offer._id == ObjectId.Empty)
                Offers.InsertOne(offer);
            else
                Offers.ReplaceOne(
                    x => x.Symbol == offer.Symbol && x.TraderId == offer.TraderId,
                    offer
                );

            Log.Information("{@offer}", offer);
        }
    }
}
