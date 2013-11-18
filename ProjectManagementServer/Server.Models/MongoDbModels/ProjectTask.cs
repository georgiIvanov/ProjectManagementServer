using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models.MongoDbModels
{
    public class ProjectTask
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId Id { get; set; }
        public string OrganizationName { get; set; }
        public string ProjectName { get; set; }
        public string TaskName { get; set; }
        public string TaskDescription { get; set; }
        public List<string> UsersParticipating { get; set; }
    }
}
