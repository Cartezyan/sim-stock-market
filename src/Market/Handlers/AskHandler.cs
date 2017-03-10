using Serilog;
using SimStockMarket.Market.Contracts;

namespace SimStockMarket.Market.Handlers
{
    public class AskHandler : Handler<Ask>
    {
        private static ILogger Log = Serilog.Log.ForContext<AskHandler>();

        private readonly StockMarket _market;
        private readonly TradeRequestHandler _tradeHandler;

        public AskHandler(StockMarket market, TradeRequestHandler tradeHandler)
        {
            _market = market;
            _tradeHandler = tradeHandler;
        }

        public override void Handle(Ask ask)
        {
            Log.Verbose("Processing {@ask}...", ask);

            var bid = _market.FindBuyer(ask);

            if (bid == null || bid.TraderId == ask.TraderId)
            {
                Log.Debug("No buyer for {symbol} @ {price} - ask submitted.", ask.Symbol, ask.Price);
                _market.SubmitOffer(ask);
            }
            else
            {
                Log.Debug("Found buyer for {symbol} @ {price} - executing trade.", ask.Symbol, ask.Price);
                _tradeHandler.Handle(ask, bid);
            }
        }

    }
}
