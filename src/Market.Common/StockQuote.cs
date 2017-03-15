using Newtonsoft.Json;
using System;

namespace SimStockMarket.Market
{
    public class StockQuote
    {
        [JsonIgnore]
        public MongoDB.Bson.ObjectId _id;
        public DateTime AsOf { get; set; }
        public string Symbol { get; set; }
        public decimal Price { get; set; }
        public decimal Bid { get; set; }
        public decimal Ask { get; set; }
    }
}
