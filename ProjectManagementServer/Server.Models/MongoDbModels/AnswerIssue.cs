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
        [StringLength(300)]
        public string Text { get; set; }
        public DateTime Time { get; set; }
        public string Username { get; set; }
    }
}
