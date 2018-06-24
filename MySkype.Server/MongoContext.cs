using MongoDB.Driver;
using MySkype.Server.Models;

namespace MySkype.Server
{
    public class MongoContext
    {
        private readonly IMongoDatabase _database;

        //public IMongoCollection<Room> Rooms => _database.GetCollection<Room>("Rooms");

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");

        public IMongoCollection<Photo> Photos => _database.GetCollection<Photo>("Photos");

        public IMongoCollection<Call> Calls => _database.GetCollection<Call>("Calls");
        
        public MongoContext()
        {
            _database = new MongoClient().GetDatabase("MySkype");
        }
    }
}
