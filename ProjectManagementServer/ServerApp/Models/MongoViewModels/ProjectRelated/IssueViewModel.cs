using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ServerApp.Models.MongoViewModels.ProjectRelated
{
    public class IssueViewModel
    {
        // has to have proj id instead
        public string ProjectName { get; set; }
        [StringLength(300)]
        public string Text { get; set; }
        public DateTime DatePosted { get; set; }
        public string ByUser { get; set; }
    }
}