using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServerApp.Models.MongoViewModels
{
    public class UserInProject
    {
        public string Username { get; set; }
        public string OrganizationName { get; set; }
        public string ProjectName { get; set; }
    }
}