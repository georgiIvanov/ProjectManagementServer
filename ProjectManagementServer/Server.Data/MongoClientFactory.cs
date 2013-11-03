using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Data
{
    public static class MongoClientFactory
    {
        static MongoClient client = new MongoClient("mongodb://appharbor_280733ac-9370-4ecd-8290-cbc41eccf323:piun6k80t8ue70irb71rneun90@ds053128.mongolab.com:53128/appharbor_280733ac-9370-4ecd-8290-cbc41eccf323");
        static MongoServer server = client.GetServer();


        public static MongoDatabase GetDatabase()
        {
            return server.GetDatabase("appharbor_280733ac-9370-4ecd-8290-cbc41eccf323");
        }
    }
}
