using MongoDB.Driver;
using MySkype.Server.Data.Models;

namespace MySkype.Server.Data
{
    public class MongoContext
    {
        private readonly IMongoDatabase _database;

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");

        public IMongoCollection<Call> Calls => _database.GetCollection<Call>("Calls");

        public MongoContext()
        {
            _database = new MongoClient().GetDatabase("MySkype");
        }
    }
}
