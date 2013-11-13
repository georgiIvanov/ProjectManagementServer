using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServerApp.Models.MongoViewModels
{
    public class ProjectForUser
    {
        public string Name { get; set; }
        public bool UserParticipatesIn { get; set; }
    }
}