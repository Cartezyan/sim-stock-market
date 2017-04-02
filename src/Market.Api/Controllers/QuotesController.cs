using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Linq;

namespace SimStockMarket.Market.Api.Controllers
{
    [Route("quotes")]
    public class QuotesController : Controller
    {
        private readonly IMongoCollection<StockQuote> _quotes;

        public QuotesController(IMongoCollection<StockQuote> quotes)
        {
            _quotes = quotes;
        }

        /// <summary>
        /// Gets all available Stock Quotes
        /// </summary>
        /// <returns>All available Stock Quotes</returns>
        [HttpGet]
        public IQueryable<StockQuote> Quotes()
        {
            return _quotes.AsQueryable();
        }

        /// <summary>
        /// Gets a Stock Quote by symbol
        /// </summary>
        /// <returns>The Stock Quote</returns>
        [HttpGet("{symbol}")]
        public StockQuote Quote(string symbol)
        {
            return _quotes.Find(x => x.Symbol == symbol).FirstOrDefault();
        }
    }
}
