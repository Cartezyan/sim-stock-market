using Serilog;

namespace SimStockMarket.Market.Handlers
{
    public class TradeRequestHandler : Handler<TradeRequest>
    {
        private static ILogger Log = Serilog.Log.ForContext<BidHandler>();

        private readonly IStockMarket _market;
        private readonly TradeLedger _ledger;

        public TradeRequestHandler(IStockMarket market, TradeLedger ledger)
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
            Log.Verbose("Processing {@tradeRequest}...", request);

            var ask = request?.Ask;
            var bid = request?.Bid;

            var trade = _ledger.ExecuteTrade(bid, ask);

            _market.DeleteOffer(ask);
            _market.DeleteOffer(bid);
        }

    }
}
