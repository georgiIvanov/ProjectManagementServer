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
        ProjectManager = 20,
        SeniorEmployee = 15,
        Employee = 10,
        JuniorEmployee = 5
    }
}