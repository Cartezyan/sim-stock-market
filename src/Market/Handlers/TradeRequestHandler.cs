using System;
using Serilog;
using MongoDB.Driver;
using StackExchange.Redis;
using Newtonsoft.Json;

namespace SimStockMarket.Market.Handlers
{
    public class TradeRequestHandler : Handler<TradeRequest>
    {
        private static ILogger Log = Serilog.Log.ForContext<BidHandler>();

        private readonly IStockMarket _market;
        private readonly IMongoDatabase _db;
        private readonly IDatabase _redis;

        public TradeRequestHandler(IStockMarket market, IMongoDatabase db, IDatabase redis)
        {
            _market = market;
            _db = db;
            _redis = redis;
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

            _db.GetCollection<Trade>().InsertOne(trade);

            _market.DeleteOffer(ask);
            _market.DeleteOffer(bid);

            _market.ReevaluateQuote(trade.Symbol);

            Log.Information("TRADE {symbol} @ {price} ({seller} => {buyer})",
                            trade.Symbol, trade.Price, trade.SellerId, trade.BuyerId);
        }

    }
}
