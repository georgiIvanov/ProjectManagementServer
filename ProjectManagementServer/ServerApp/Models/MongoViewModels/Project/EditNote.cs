using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServerApp.Models.MongoViewModels.Project
{
    public class EditNote
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string Title { get; set; }
    }
}