using MongoDB.Driver;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace SimStockMarket.Extensions.MongoDb
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddMongoDb(this IServiceCollection services, string url, string database)
        {
            services.AddSingleton<IMongoDatabase>(_ => {
                var mongodb = new MongoClient(url);
                var db = mongodb.GetDatabase(database);
                return db;
            });

            return services;
        }

        public static IServiceCollection AddMongoCollection<T>(this IServiceCollection services)
        {
            return services.AddScoped(GetCollection<T>);
        }

        private static IMongoCollection<T> GetCollection<T>(IServiceProvider services)
        {
            var db = services.GetService<IMongoDatabase>();
            return db.GetCollection<T>(typeof(T).Name);
        }

    }
}
