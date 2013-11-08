using MongoDB.Bson;
using Newtonsoft.Json;
using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServerApp.Models.MongoViewModels
{
    public class OgranizationListEntry
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId OrganizationId { get; set; }
        public string Name { get; set; }

        public UserRoles Role { get; set; }
    }
}