using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace SimStockMarket.Market.Api.Controllers
{
    [Route("symbols")]
    public class SymbolsController : Controller
    {
        private readonly IStockMarket _market;

        public SymbolsController(IStockMarket market)
        {
            _market = market;
        }

        [HttpGet("")]
        public IEnumerable<StockSymbol> Symbols()
        {
            return _market.GetSymbols();
        }

        [HttpGet("{symbol}", Name = "GetSymbol")]
        public StockSymbol Symbol(string symbol)
        {
            return _market.GetSymbol(symbol);
        }

        [HttpPost("")]
        public IActionResult Post([FromBody]StockSymbol symbol)
        {
            _market.AddSymbol(symbol);
            return Ok();
        }

        [HttpPut("{symbol}")]
        public IActionResult Put(string symbol, [FromBody]StockSymbol update)
        {
            _market.UpdateSymbol(update);
            return Ok();
        }

        [HttpDelete("{symbol}")]
        public IActionResult Delete(string symbol)
        {
            return Ok();
        }
    }
}
