using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace SimStockMarket.TraderBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var traderId = Environment.MachineName;

            Console.WriteLine($"Starting Trade Bot {traderId}...");

            var config = new ConfigurationBuilder()
                        .AddInMemoryCollection(new Dictionary<string, string>
                        {
                            { "QueueHostName", "localhost" },
                        })
                        .AddEnvironmentVariables()
                        .Build();

            var queueHost = config["QueueHostName"];

            Debug.WriteLine($"Connecting to message host {queueHost}...");

            using (var connection = MessageQueue.Connect(queueHost))
            using (var queue = MessageQueue.Connect(connection, "stockmarket"))
            {
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