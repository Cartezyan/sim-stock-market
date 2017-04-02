using System;
using System.Linq;
using Serilog;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace SimStockMarket.Market.Handlers
{
    public class TradeRequestHandler : Handler<TradeRequest>
    {
        private static ILogger Log = Serilog.Log.ForContext<BidHandler>();

        private readonly IMessageBus _bus;
        private readonly IMongoCollection<Trade> _trades;
        private readonly IMongoCollection<StockQuote> _quotes;
        private readonly IMongoCollection<TradeOffer> _offers;

        public TradeRequestHandler(
                IMessageBus bus,
                IMongoCollection<Trade> trades,
                IMongoCollection<TradeOffer> offers,
                IMongoCollection<StockQuote> quotes
            )
        {
            _bus = bus;
            _offers = offers;
            _trades = trades;
            _quotes = quotes;
        }

        public void Handle(Ask ask, Bid bid)
        {
            Handle(new TradeRequest(ask, bid));
        }

        public override void Handle(TradeRequest request)
        {
            Log.Verbose("Processing {@tradeRequest}...", request);

            var ask = request?.Ask;
            var bid = request?.Bid;

            Log.Debug("Received {@bid} / {@ask}", bid, ask);

            if (bid == null)
                throw new ArgumentNullException(nameof(bid));
            if (ask == null)
                throw new ArgumentNullException(nameof(ask));

            if (!string.Equals(bid.Symbol, ask.Symbol, StringComparison.OrdinalIgnoreCase))
                throw new Exception($"Bid and Ask are for two different stocks! ({bid.Symbol} and {ask.Symbol})");

            if (ask.Price > bid.Price)
            {
                throw new Exception($"Ask {ask.Price} is greater than bid {bid.Price}");
            }

            var trade = new Trade(ask.Symbol, ask.Price, ask.TraderId, bid.TraderId);

            _trades.InsertOne(trade);

            DeleteOffer(ask);
            DeleteOffer(bid);

            ReevaluateQuote(trade.Symbol);

            Log.Information("TRADE {symbol} @ {price} ({seller} => {buyer})",
                            trade.Symbol, trade.Price, trade.SellerId, trade.BuyerId);
        }

        protected internal void ReevaluateQuote(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                throw new ArgumentNullException(nameof(symbol));

            var quote = GetQuote(symbol) ?? new StockQuote { Symbol = symbol };

            quote.AsOf = DateTime.UtcNow;
            quote.Ask = GetAsk(symbol);
            quote.Bid = GetBid(symbol);
            quote.Price = GetPrice(symbol);

            if (quote._id == null)
            {
                _quotes.InsertOne(quote);
            }
            else
            {
                _quotes.ReplaceOne(x => x._id == quote._id, quote);
            }

            _bus.Publish("quote", quote);

            Log.Information("{@quote}", quote);
        }

        decimal? GetPrice(string symbol)
        {
            return _trades.AsQueryable()
                .Where(x => x.Symbol == symbol)
                .OrderByDescending(x => x.Timestamp)
                .Select(x => x.Price)
                .FirstOrDefault();
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

        StockQuote GetQuote(string symbol)
        {
            return _quotes.Find(x => x.Symbol == symbol).FirstOrDefault();
        }

        IQueryable<TradeOffer> GetOffersBySymbol(string symbol)
        {
            return _offers.AsQueryable().Where(x => x.Symbol == symbol);
        }

        void DeleteOffer(TradeOffer offer)
        {
            Log.Verbose("Resolving {@offer}...", offer);

            _offers.DeleteOne(x =>
                  x.TraderId == offer.TraderId
                && x.Symbol == offer.Symbol
            );

            _bus.Publish("offer_deleted", new { offer.Symbol, offer.TraderId });

            Log.Debug("Resolved {@offer}", offer);
        }

    }
}
