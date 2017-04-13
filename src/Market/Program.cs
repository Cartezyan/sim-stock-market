using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using Serilog.Events;
using SimStockMarket.Extensions.Redis;
using SimStockMarket.Extensions.MongoDb;
using SimStockMarket.Extensions.RabbitMQ;
using SimStockMarket.Market.Handlers;

namespace SimStockMarket.Market
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = Configure();
            var services = ConfigureServices(config);

            StartMarket(services);
        }

        static void StartMarket(IServiceProvider services)
        {
            Log.Information("==== Stock Market ====");

            var queue =
                services.GetService<IMessageQueue>()
                    .Subscribe<Ask>(ask => services.GetService<AskHandler>().Handle(ask))
                    .Subscribe<Bid>(bid => services.GetService<BidHandler>().Handle(bid));

            Log.Information("Market started");

            while (true)
            {
                Thread.Sleep(100);
            }
        }

        static IConfiguration Configure()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> {
                    { "MongoUrl", "mongodb://localhost:27017" },
                    { "MongoDatabase", "stockmarket" },
                    { "QueueHost", "localhost" },
                    { "QueueName", "stockmarket" },
                    { "RedisHost", "localhost" },
                    { "LogLevel", LogEventLevel.Information.ToString() },
                })
                .AddEnvironmentVariables()
                .Build();

            var logLevel = LogEventLevel.Warning;

            Enum.TryParse(config["LogLevel"], out logLevel);

            var logLevelSwitch = new Serilog.Core.LoggingLevelSwitch(logLevel);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(logLevelSwitch)
                .WriteTo.LiterateConsole()
                .CreateLogger();

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            return config;
        }

        static IServiceProvider ConfigureServices(IConfiguration config)
        {
            return new ServiceCollection()
                .AddRedis(config["RedisHost"])
                .AddMessageQueue(config["QueueHost"], config["QueueName"])
                .AddMongoDb(config["MongoUrl"], config["MongoDatabase"])
                    .AddMongoCollection<StockQuote>()
                    .AddMongoCollection<StockSymbol>()
                    .AddMongoCollection<Trade>()
                    .AddMongoCollection<TradeOffer>()
                .AddTransient<AskHandler>()
                .AddTransient<BidHandler>()
                .AddTransient<TradeRequestHandler>()
                .BuildServiceProvider();
        }
    }
}