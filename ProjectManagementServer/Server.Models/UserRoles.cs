using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public enum UserRoles
    {
        OrganizationOwner = 1,
        OrganizationManager = 2,
        TeamManager = 3,
        Employee = 4
    }
}