namespace SimStockMarket.Market.Contracts
{
    public class TradeRequest
    {
        public Ask Ask { get; set; }
        public Bid Bid { get; set; }

        public TradeRequest()
        {
        }

        public TradeRequest(Ask ask, Bid bid)
        {
            Ask = ask;
            Bid = bid;
        }
    }
}
