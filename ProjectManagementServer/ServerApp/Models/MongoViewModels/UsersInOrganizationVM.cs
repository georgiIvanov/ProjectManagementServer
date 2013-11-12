using MongoDB.Bson;
using Newtonsoft.Json;
using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServerApp.Models.MongoViewModels
{
    public class UsersInOrganizationVM
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId Id { get; set; }
        public string Username { get; set; }
        public UserRoles Role { get; set; }
    }
}