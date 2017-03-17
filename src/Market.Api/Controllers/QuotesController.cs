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

        /// <summary>
        /// Gets all available Stock Quotes
        /// </summary>
        /// <returns>All available Stock Quotes</returns>
        [HttpGet]
        public IEnumerable<StockQuote> Quotes()
        {
            return market.GetQuotes();
        }

        /// <summary>
        /// Gets a Stock Quote by symbol
        /// </summary>
        /// <returns>The Stock Quote</returns>
        [HttpGet("{symbol}")]
        public StockQuote Quote(string symbol)
        {
            return market.GetQuote(symbol);
        }
    }
}
