using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
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

        public async Task WriteWoodRecords(List<DBWoodRecord> data)
        {
            var collection = database.GetCollection<BsonDocument>("WoodRecords");
            var documents = new List<BsonDocument>();

            foreach(var x in data)
            {
                var document = new BsonDocument
            {
                { "woodID", x.woodID },
                { "treeID", x.treeID },
                { "x", x.x },
                { "y", x.y }
            };
                documents.Add(document);
            }

            try
            {
                await collection.InsertManyAsync(documents);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task WriteMonkeyRecords(List<DBMonkeyRecord> data)
        {
            var collection = database.GetCollection<BsonDocument>("MonkeyRecords");
            var documents = new List<BsonDocument>();

            foreach(var x in data)
            {
                var document = new BsonDocument
            {
                { "monkeyID", x.monkeyID },
                { "monkeyName", x.monkeyName },
                { "woodID", x.woodID },
                { "seqNr", x.seqNr },
                { "treeID", x.treeID },
                { "x", x.x },
                { "y", x.y }
            };
                documents.Add(document);
            }

            try
            {
                await collection.InsertManyAsync(documents);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
