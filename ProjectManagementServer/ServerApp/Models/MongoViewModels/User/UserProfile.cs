using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServerApp.Models.MongoViewModels.User
{
    public class UserProfile
    {
        public string OrganizationName { get; set; }
        public UserRoles UserRole { get; set; }
        public string Username { get; set; }
    }
}