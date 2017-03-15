using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Bson;
using Serilog;

namespace SimStockMarket.Market
{
    public interface IStockMarket
    {
        void AddSymbol(StockSymbol symbol);
        void DeleteOffer(TradeOffer offer);
        IEnumerable<TradeOffer> GetOffersBySymbol(string symbol);
        StockSymbol GetSymbol(string symbol);
        IEnumerable<StockSymbol> GetSymbols();
        StockQuote GetQuote(string symbol);
        IEnumerable<StockQuote> GetQuotes();
        void SubmitOffer(TradeOffer offer);
        void UpdateSymbol(StockSymbol symbol);
    }

    public class StockMarket : IStockMarket
    {
        private static ILogger Log = Serilog.Log.ForContext<IStockMarket>();

        private readonly IDictionary<Type, object> Collections = new Dictionary<Type, object>();
        private readonly IMongoDatabase _db;

        public StockMarket(IMongoDatabase db)
        {
            _db = db;
        }

        public IEnumerable<TradeOffer> GetOffersBySymbol(string symbol)
        {
            return Collection<TradeOffer>().AsQueryable().Where(x => x.Symbol == symbol);
        }

        public void AddSymbol(StockSymbol symbol)
        {
            var existing = GetSymbol(symbol.Symbol);

            if(existing == null)
                Collection<StockSymbol>().InsertOne(symbol);
        }

        public void DeleteOffer(TradeOffer offer)
        {
            Log.Verbose("Resolving {@offer}...", offer);

            Collection<TradeOffer>().DeleteOne(x =>
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
                Collection<TradeOffer>().InsertOne(offer);
            else
                Collection<TradeOffer>().ReplaceOne(
                    x => x.Symbol == offer.Symbol && x.TraderId == offer.TraderId,
                    offer
                );

            Log.Information("{@offer}", offer);
        }

        public StockSymbol GetSymbol(string symbol)
        {
            return Collection<StockSymbol>().AsQueryable().FirstOrDefault(x => x.Symbol == symbol);
        }

        public IEnumerable<StockSymbol> GetSymbols()
        {
            return Collection<StockSymbol>().AsQueryable().ToArray();
        }

        public StockQuote GetQuote(string symbol)
        {
            return Collection<StockQuote>().AsQueryable().FirstOrDefault(x => x.Symbol == symbol);
        }

        public IEnumerable<StockQuote> GetQuotes()
        {
            return Collection<StockQuote>().AsQueryable().ToArray();
        }

        private IMongoCollection<T> Collection<T>()
        {
            var type = typeof(T);

            if (!Collections.ContainsKey(type))
            {
                Collections[type] = _db.GetCollection<T>(type.Name);
            }

            return (IMongoCollection<T>)Collections[type];
        }

        public void UpdateSymbol(StockSymbol symbol)
        {
            if (symbol == null)
                throw new ArgumentNullException(nameof(symbol));

            var existing = GetSymbol(symbol.Symbol);

            if (existing == null)
                throw new Exception($"Symbol {symbol.Symbol} does not exist");

            existing.Name = symbol.Name;

            Collection<StockSymbol>().ReplaceOne(x => x._id == existing._id, existing);
        }
    }
}
