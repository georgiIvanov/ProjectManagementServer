﻿using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models.MongoDbModels
{
    public class AnswerIssue
    {
        public string Text { get; set; }
        public DateTime Time { get; set; }
        public string Username { get; set; }
        [BsonIgnore]
        public string IssueId { get; set; }
    }
}
