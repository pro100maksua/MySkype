using MongoDB.Driver;
using MySkype.Server.Models;

namespace MySkype.Server
{
    public class MongoContext
    {
        private IMongoDatabase _database;

        //public IMongoCollection<Room> Rooms => _database.GetCollection<Room>("Rooms");

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");

        public IMongoCollection<Photo> Photos => _database.GetCollection<Photo>("Photos");

        public MongoContext()
        {
            _database = new MongoClient().GetDatabase("MySkype");
        }
    }
}
