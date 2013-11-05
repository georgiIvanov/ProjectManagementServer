using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models.MongoDbModels
{
    public class UsersOrganizations
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId Id { get; set; }
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId UserId { get; set; }
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId OrganizationId { get; set; }
        public string Name { get; set; }

        public UserRoles Role { get; set; }
    }
}
