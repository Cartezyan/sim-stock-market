using System;
using SimStockMarket.Market.Contracts;
using System.Diagnostics;

namespace SimStockMarket.Market.Handlers
{
    public class AskHandler : Handler<Ask>
    {
        private readonly StockMarket _market;
        private readonly TradeRequestHandler _tradeHandler;

        public AskHandler(StockMarket market, TradeRequestHandler tradeHandler)
        {
            _market = market;
            _tradeHandler = tradeHandler;
        }

        public override void Handle(Ask ask)
        {
            Console.WriteLine($"[ASK] {ask.Symbol} @ {ask.Price} ({ask.TraderId})");

            var bid = _market.FindBuyer(ask);

            if (bid == null)
            {
                Debug.WriteLine($"No buyer for {bid.Symbol} @ {bid.Price} - ask submitted.");
                _market.SubmitAsk(ask);
            }
            else
            {
                Debug.WriteLine($"Found buyer for {ask.Symbol} @ {ask.Price}");
                _tradeHandler.Handle(ask, bid);
            }
        }

    }
}
