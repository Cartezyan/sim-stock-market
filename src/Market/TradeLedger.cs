using System;
using System.Collections.Generic;
using Serilog;
using SimStockMarket.Market.Contracts;

namespace SimStockMarket.Market
{
    public class TradeLedger
    {
        private static ILogger Log = Serilog.Log.ForContext<TradeLedger>();

        private readonly IList<Trade> Trades = new List<Trade>();

        public Trade ExecuteTrade(Bid bid, Ask ask)
        {
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

            Trades.Add(trade);

            Log.Information("TRADE {symbol} @ {price} ({seller} => {buyer})", 
                            trade.Symbol, trade.Price, trade.SellerId, trade.BuyerId);

            return trade;
        }
    }
}
