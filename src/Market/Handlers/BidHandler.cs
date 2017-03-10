using Serilog;
using SimStockMarket.Market.Contracts;

namespace SimStockMarket.Market.Handlers
{
    public class BidHandler : Handler<Bid>
    {
        private static ILogger Log = Serilog.Log.ForContext<BidHandler>();

        private readonly StockMarket _market;
        private readonly TradeRequestHandler _tradeHandler;

        public BidHandler(StockMarket market, TradeRequestHandler tradeHandler)
        {
            _market = market;
            _tradeHandler = tradeHandler;
        }

        public override void Handle(Bid bid)
        {
            Log.Verbose("Processing {@bid}...", bid);

            var ask = _market.FindSeller(bid);

            if (ask == null || bid.TraderId == ask.TraderId)
            {
                Log.Debug("No seller for {symbol} @ {price} - bid submitted.", bid.Symbol, bid.Price);
                _market.SubmitOffer(bid);
            }
            else
            {
                Log.Debug("Found seller for {symbol} @ {price} - executing trade", ask.Symbol, ask.Price);
                _tradeHandler.Handle(ask, bid);
            }
        }
    }
}
