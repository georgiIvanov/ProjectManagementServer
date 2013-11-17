using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServerApp.Models.MongoViewModels.User
{
    public class SetAsProjectManager
    {
        public string ProjectName { get; set; }
        public string Username { get; set; }
        public string OrganizationName { get; set; }
        public bool SetAsManager { get; set; }
    }
}