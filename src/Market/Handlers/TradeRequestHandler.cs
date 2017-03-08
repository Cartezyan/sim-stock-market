using System;
using SimStockMarket.Market.Contracts;

namespace SimStockMarket.Market.Handlers
{
    public class TradeRequestHandler : Handler<TradeRequest>
    {
        private readonly StockMarket _market;
        private readonly TradeLedger _ledger;

        public TradeRequestHandler(StockMarket market, TradeLedger ledger)
        {
            _market = market;
            _ledger = ledger;
        }

        public void Handle(Ask ask, Bid bid)
        {
            Handle(new TradeRequest(ask, bid));
        }

        public override void Handle(TradeRequest request)
        {
            var ask = request?.Ask;
            var bid = request?.Bid;

            var trade = _ledger.ExecuteTrade(bid, ask);

            _market.ResolveAsk(ask, trade.Id);
            _market.ResolveBid(bid, trade.Id);

            Console.WriteLine($"[TRADE] {trade.Symbol} @ {trade.Price} ({trade.SellerId} => {trade.BuyerId})");
        }
    }
}
