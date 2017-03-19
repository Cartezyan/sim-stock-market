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
        void ReevaluateQuote(string symbol);
        void UpdateSymbol(StockSymbol symbol);
    }

    public class StockMarket : IStockMarket
    {
        private static ILogger Log = Serilog.Log.ForContext<IStockMarket>();

        private readonly IMongoDatabase _db;
        private readonly IMessageBus _bus;

        public StockMarket(IMongoDatabase db, IMessageBus bus)
        {
            _db = db;
            _bus = bus;
        }

        public IEnumerable<TradeOffer> GetOffersBySymbol(string symbol)
        {
            return _db.GetCollection<TradeOffer>().AsQueryable().Where(x => x.Symbol == symbol);
        }

        public void AddSymbol(StockSymbol symbol)
        {
            var existing = GetSymbol(symbol.Symbol);

            if(existing == null)
                _db.GetCollection<StockSymbol>().InsertOne(symbol);

            _bus.Publish("symbol", symbol);
        }

        public void DeleteOffer(TradeOffer offer)
        {
            Log.Verbose("Resolving {@offer}...", offer);

            _db.GetCollection<TradeOffer>().DeleteOne(x =>
                  x.TraderId == offer.TraderId
                && x.Symbol == offer.Symbol
            );

            _bus.Publish("offer_deleted", new { offer.Symbol, offer.TraderId });

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
            {
                _db.GetCollection<TradeOffer>().InsertOne(offer);
            }
            else
            {
                _db.GetCollection<TradeOffer>().ReplaceOne(
                    x => x.Symbol == offer.Symbol && x.TraderId == offer.TraderId,
                    offer
                );
            }

            _bus.Publish("offer", offer);

            ReevaluateQuote(offer.Symbol);

            Log.Information("{@offer}", offer);
        }

        public StockSymbol GetSymbol(string symbol)
        {
            return _db.GetCollection<StockSymbol>().AsQueryable().FirstOrDefault(x => x.Symbol == symbol);
        }

        public IEnumerable<StockSymbol> GetSymbols()
        {
            return _db.GetCollection<StockSymbol>().AsQueryable().ToArray();
        }

        public StockQuote GetQuote(string symbol)
        {
            return _db.GetCollection<StockQuote>().AsQueryable().FirstOrDefault(x => x.Symbol == symbol);
        }

        public IEnumerable<StockQuote> GetQuotes()
        {
            return _db.GetCollection<StockQuote>().AsQueryable().ToArray();
        }

        public void ReevaluateQuote(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                throw new ArgumentNullException(nameof(symbol));

            var quote = GetQuote(symbol)
                        ?? new StockQuote { Symbol = symbol };

            quote.AsOf = DateTime.UtcNow;
            quote.Ask = GetAsk(symbol);
            quote.Bid = GetBid(symbol);
            quote.Price = GetPrice(symbol);

            if (quote._id == null)
            {
                _db.GetCollection<StockQuote>().InsertOne(quote);
            }
            else
            {
                _db.GetCollection<StockQuote>().ReplaceOne(x => x._id == quote._id, quote);
            }

            _bus.Publish("quote", quote);

            Log.Information("{@quote}", quote);
        }

        public void UpdateSymbol(StockSymbol symbol)
        {
            if (symbol == null)
                throw new ArgumentNullException(nameof(symbol));

            var existing = GetSymbol(symbol.Symbol);

            if (existing == null)
                throw new Exception($"Symbol {symbol.Symbol} does not exist");

            existing.Name = symbol.Name;

            _db.GetCollection<StockSymbol>().ReplaceOne(x => x._id == existing._id, existing);

            _bus.Publish("symbol", symbol);
        }

        decimal? GetAsk(string symbol)
        {
            return GetOffersBySymbol(symbol)
                .OfType<Ask>()
                .OrderByDescending(x => x.Timestamp)
                .Select(x => x.Price)
                .FirstOrDefault();
        }

        decimal? GetBid(string symbol)
        {
            return GetOffersBySymbol(symbol)
                .OfType<Bid>()
                .OrderByDescending(x => x.Timestamp)
                .Select(x => x.Price)
                .FirstOrDefault();
        }

        decimal? GetPrice(string symbol)
        {
            return _db.GetCollection<Trade>().AsQueryable()
                .Where(x => x.Symbol == symbol)
                .OrderByDescending(x => x.Timestamp)
                .Select(x => x.Price)
                .FirstOrDefault();
        }
    }
}
