using Microsoft.AspNetCore.Mvc;

namespace SimStockMarket.Market.Api.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        [HttpGet]
        public string Get()
        {
            return $"Stock Market API:\r\n" +
                $"\t/quotes\r\n" +
                $"\t/symbols\r\n";
        }
    }
}
