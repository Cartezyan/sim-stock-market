using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;

namespace SimStockMarket.Market.Api.Controllers
{
    [Route("symbols")]
    public class SymbolsController : Controller
    {
        private readonly IMongoCollection<StockSymbol> _symbols;

        public SymbolsController(IMongoCollection<StockSymbol> symbols)
        {
            _symbols = symbols;
        }

        /// <summary>
        /// Gets all available Stock Symbols
        /// </summary>
        /// <returns>All available Stock Symbols</returns>
        [HttpGet("")]
        public IEnumerable<StockSymbol> Symbols()
        {
            return _symbols.AsQueryable();
        }

        /// <summary>
        /// Gets a Stock Symbol by symbol
        /// </summary>
        /// <returns>The Stock Symbol</returns>
        [HttpGet("{symbol}", Name = "GetSymbol")]
        public StockSymbol Symbol(string symbol)
        {
            return _symbols.Find(x => x.Symbol == symbol).FirstOrDefault();
        }

        /// <summary>
        /// Registers a new Stock Symbol to be available for trading
        /// </summary>
        /// <param name="symbol">Details of the Stock Symbol to add</param>
        [HttpPost("")]
        public IActionResult Post([FromBody]StockSymbol symbol)
        {
            Upsert(symbol);
            return Ok();
        }

        /// <summary>
        /// Updates details of an existing Stock Symbol
        /// </summary>
        /// <param name="symbol">Symbol to update</param>
        /// <param name="update">Details of the Stock Symbol updates</param>
        [HttpPut("{symbol}")]
        public IActionResult Put(string symbol, [FromBody]StockSymbol update)
        {
            update.Symbol = symbol;
            Upsert(update);
            return Ok();
        }

        /// <summary>
        /// Removes a Stock Symbol from the system so it is no longer available for trading
        /// </summary>
        /// <param name="symbol">The Symbol to remove</param>
        [HttpDelete("{symbol}")]
        public IActionResult Delete(string symbol)
        {
            _symbols.DeleteOne(x => x.Symbol == symbol);
            return Ok();
        }

        void Upsert(StockSymbol symbol)
        {
            if (symbol._id == ObjectId.Empty)
            {
                _symbols.InsertOne(symbol);
            }
            else
            {
                _symbols.ReplaceOne(x => x.Symbol == symbol.Symbol, symbol);
            }
        }
    }
}
