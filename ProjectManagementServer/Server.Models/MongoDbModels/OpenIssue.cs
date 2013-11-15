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
    public class OpenIssue
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId Id { get; set; }
        // has to have proj id instead
        public string ProjectName { get; set; }
        [StringLength(30)]
        public string Title { get; set; }
        [StringLength(300)]
        public string Text { get; set; }
        public DateTime DatePosted { get; set; }
        public string ByUser { get; set; }

        public List<AnswerIssue> Answers { get; set; }

    }
}
