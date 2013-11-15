using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models.MongoDbModels
{
    public class UserInProject
    {
        //[JsonConverter(typeof(ObjectIdConverter))]
        //public ObjectId ProjectId { get; set; }
        public string Username { get; set; }
        public string OrganizationName { get; set; }
        public string ProjectName { get; set; }
    }
}
