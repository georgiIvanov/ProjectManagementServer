using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public enum UserRoles
    {
        OrganizationOwner = 50,
        OrganizationManager = 30,
        TeamManager = 20,
        Employee = 10
    }
}