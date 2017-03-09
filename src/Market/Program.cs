using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimStockMarket.Market.Handlers;
using SimStockMarket.Market.Contracts;

namespace SimStockMarket.Market
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("==== Stock Market ====");

            var config = Configure();
            var services = ConfigureServices(config);

            using (var connection = MessageQueue.Connect(config["QueueHostName"]))
            using (var queue = MessageQueue.Connect(connection, "stockmarket"))
            {
                queue.Subscribe<Ask>(ask => services.GetService<AskHandler>().Handle(ask));
                queue.Subscribe<Bid>(bid => services.GetService<BidHandler>().Handle(bid));

                Console.WriteLine("Market started");

                while (true)
                {
                    Thread.Sleep(100);
                }
            }
        }

        static IConfiguration Configure()
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> {
                    { "QueueHostName", "localhost" },
                })
                .AddEnvironmentVariables()
                .Build();
        }

        static IServiceProvider ConfigureServices(IConfiguration config)
        {
            return new ServiceCollection()
                .AddSingleton<StockMarket>()
                .AddSingleton<TradeLedger>()
                .AddTransient<AskHandler>()
                .AddTransient<BidHandler>()
                .AddTransient<TradeRequestHandler>()
                .BuildServiceProvider();
        }
    }
}