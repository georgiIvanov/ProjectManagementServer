using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        [StringLength(20, MinimumLength=1)]
        public string TaskName { get; set; }
        public string TaskDescription { get; set; }
        public List<string> UsersParticipating { get; set; }
        public bool Completed { get; set; }
    }
}
