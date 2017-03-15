using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace SimStockMarket.Market.Handlers
{
    public class AskHandler : Handler<Ask>
    {
        private static ILogger Log = Serilog.Log.ForContext<AskHandler>();

        private readonly IStockMarket _market;
        private readonly TradeRequestHandler _tradeHandler;

        public AskHandler(IStockMarket market, TradeRequestHandler tradeHandler)
        {
            _market = market;
            _tradeHandler = tradeHandler;
        }

        public override void Handle(Ask ask)
        {
            Log.Verbose("Processing {@ask}...", ask);

            var bid = FindBuyer(ask);

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

        internal Bid FindBuyer(Ask ask)
        {
            Log.Verbose("Finding buyer for {@ask}...", ask);

            return _market.GetOffersBySymbol(ask?.Symbol)
                .OfType<Bid>()
                .OrderBy(x => x.Price)
                .ThenBy(x => x.Timestamp)
                .FirstOrDefault(x => x.Price >= ask?.Price);
        }
    }
}
