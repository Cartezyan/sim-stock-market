using MongoDB.Driver;

namespace SimStockMarket
{
    public static class MongoDatabaseExtensions
    {
        public static IMongoCollection<T> GetCollection<T>(this IMongoDatabase db)
        {
            return db.GetCollection<T>(typeof(T).Name);
        }
    }
}
