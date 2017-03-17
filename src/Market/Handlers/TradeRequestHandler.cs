using System;
using Serilog;
using MongoDB.Driver;

namespace SimStockMarket.Market.Handlers
{
    public class TradeRequestHandler : Handler<TradeRequest>
    {
        private static ILogger Log = Serilog.Log.ForContext<BidHandler>();

        private readonly IStockMarket _market;
        private readonly IMongoDatabase _db;

        public TradeRequestHandler(IStockMarket market, IMongoDatabase db)
        {
            _market = market;
            _db = db;
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

            Log.Information("TRADE {symbol} @ {price} ({seller} => {buyer})",
                            trade.Symbol, trade.Price, trade.SellerId, trade.BuyerId);

            _market.DeleteOffer(ask);
            _market.DeleteOffer(bid);
        }

    }
}
