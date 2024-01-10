using EscapeFromTheWoods.Objects;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace EscapeFromTheWoods
{
    public class DBwriter
    {
        private MongoClient client;
        private IMongoDatabase database;

        public DBwriter(string connectionString)
        {
            client = new MongoClient(connectionString);
            database = client.GetDatabase("EscapeFromTheWoodsDB");
        }

        public async Task WriteWoodRecords(GameWoodRecord data)
        {
            var collection = database.GetCollection<GameWoodRecord>("WoodRecords");

            await collection.InsertOneAsync(data);
        }

        public async Task WriteMonkeyRecords(GameMonkeyRecord data)
        {
            var collection = database.GetCollection<GameMonkeyRecord>("MonkeyRecords");
            await collection.InsertOneAsync(data);
        }
    }
}
