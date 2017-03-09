using System;
using SimStockMarket.Market.Contracts;
using System.Diagnostics;

namespace SimStockMarket.Market.Handlers
{
    public class BidHandler : Handler<Bid>
    {
        private readonly StockMarket _market;
        private readonly TradeRequestHandler _tradeHandler;

        public BidHandler(StockMarket market, TradeRequestHandler tradeHandler)
        {
            _market = market;
            _tradeHandler = tradeHandler;
        }

        public override void Handle(Bid bid)
        {
            Console.WriteLine($"[BID] {bid.Symbol} @ {bid.Price} ({bid.TraderId})");

            var ask = _market.FindSeller(bid);

            if (ask == null || bid.TraderId == ask.TraderId)
            {
                Debug.WriteLine($"No seller for {bid.Symbol} @ {bid.Price} - bid submitted.");
                _market.SubmitOffer(bid);
            }
            else
            {
                Debug.WriteLine($"Found seller for {ask.Symbol} @ {ask.Price}");
                _tradeHandler.Handle(ask, bid);
            }
        }
    }
}
