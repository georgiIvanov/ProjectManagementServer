using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models.MongoDbModels
{
    public class UsersProjects
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId Id { get; set; }
        //[JsonConverter(typeof(ObjectIdConverter))]
        //public ObjectId UserId { get; set; }
        //[JsonConverter(typeof(ObjectIdConverter))]
        //public ObjectId ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Username { get; set; }
        public UserRoles Role { get; set; }
    }
}
