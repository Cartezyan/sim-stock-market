using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using SimStockMarket.Extensions.RabbitMQ;

namespace SimStockMarket.TraderBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                        .AddInMemoryCollection(new Dictionary<string, string>
                        {
                            { "QueueHostName", "localhost" },
                            { "LogLevel", LogEventLevel.Information.ToString() }
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

            var traderId = Guid.NewGuid().ToString("n").Substring(0, 8);

            Log.Information("Starting Trade Bot {traderId}...", traderId);

            var queueHost = config["QueueHost"];

            Log.Verbose("Connecting to message host {queueHost}...", queueHost);

            using (var connection = MessageQueueFactory.Connect(queueHost))
            using (var channel = connection.CreateModel())
            {
                var queue = MessageQueueFactory.CreateMessageQueue(channel, config["QueueName"]);
                var trader = new TradeGenerator(traderId);

                while (true)
                {
                    var trade = trader.GenerateTrade();

                    queue.Publish(trade.Type.ToString().ToLower(), trade);

                    Thread.Sleep(1000);
                }
            }
        }
    }
}