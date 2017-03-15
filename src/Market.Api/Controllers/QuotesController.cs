using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace SimStockMarket.Market.Api.Controllers
{
    [Route("quotes")]
    public class QuotesController : Controller
    {
        private readonly IStockMarket market;

        public QuotesController(IStockMarket market)
        {
            this.market = market;
        }

        [HttpGet]
        public IEnumerable<StockQuote> Quotes()
        {
            return market.GetQuotes();
        }

        [HttpGet("{symbol}")]
        public StockQuote Quote(string symbol)
        {
            return market.GetQuote(symbol);
        }
    }
}
