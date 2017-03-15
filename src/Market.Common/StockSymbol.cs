using Newtonsoft.Json;

namespace SimStockMarket.Market
{
    public class StockSymbol
    {
        [JsonIgnore]
        public MongoDB.Bson.ObjectId _id;
        public string Symbol { get; set; }
        public string Name { get; set; }
    }
}
