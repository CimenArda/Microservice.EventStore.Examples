using MongoDB.Driver;
using Shared.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Services
{
    public class MongoDbService : IMongoDbService
    {
        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            var database = GetDatabase();

            return database.GetCollection<T>(collectionName);

        }

        public IMongoDatabase GetDatabase(string databaseName = "ProductDB", string connectionString = "mongodb://localhost:27017")
        {
            var mongoClient = new MongoClient(connectionString);

            return mongoClient.GetDatabase(databaseName);
        }
    }
}
