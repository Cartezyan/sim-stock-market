using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;
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

            var logLevel = LogEventLevel.Warning;

            Enum.TryParse(config["LogLevel"], out logLevel);

            var logLevelSwitch = new Serilog.Core.LoggingLevelSwitch(logLevel);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(logLevelSwitch)
                .WriteTo.LiterateConsole()
                .CreateLogger();

            Log.Information("==== Stock Market ====");

            var services = ConfigureServices(config);
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
            return new ConfigurationBuilder()
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
        }

        static IServiceProvider ConfigureServices(IConfiguration config)
        {
            return new ServiceCollection()
                .AddRedis(config["RedisHost"])
                .AddMessageQueue(config["QueueHost"], config["QueueName"])
                .AddMongoDb(config["MongoUrl"], config["MongoDatabase"])
                .AddSingleton<IStockMarket, StockMarket>()
                .AddTransient<AskHandler>()
                .AddTransient<BidHandler>()
                .AddTransient<TradeRequestHandler>()
                .BuildServiceProvider();
        }

        private static IDatabase GetRedisDatabase(IServiceProvider services)
        {
            var redis = services.GetService<ConnectionMultiplexer>();
            return redis.GetDatabase();
        }

        static ConnectionMultiplexer ConnectToRedis(IServiceProvider services)
        {
            var config = services.GetService<IConfiguration>();
            var hostIp = Dns.GetHostAddressesAsync(config["RedisHost"]).Result[0].ToString();
            return ConnectionMultiplexer.Connect(hostIp);
        }

        static IMongoDatabase ConnectToMongoDatabase(IServiceProvider services)
        {
            var config = services.GetService<IConfiguration>();
            var mongodb = new MongoClient(config["MongoUrl"]);
            var db = mongodb.GetDatabase(config["MongoDatabase"]);
            return db;
        }
    }
}