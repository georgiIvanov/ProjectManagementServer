﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServerApp.Utilities
{
    public static class MongoCollections
    {
        public static string UsersInOrganizations 
        {
            get { return "UsersInOrganizations"; } 
            set { } 
        }

        public static string Organizations
        {
            get { return "Organizations"; }
            set { }
        }

        public static string Users
        {
            get { return "Users"; }
            set { }
        }

        public static string Projects
        {
            get { return "Projects"; }
            set { }
        }

        public static string Invitations
        {
            get { return "Invitations"; }
            set { }
        }

        public static string UsersInProjects
        {
            get { return "UsersInProjects"; }
            set { }
        }

        public static string Issues
        {
            get { return "Issues"; }
            set { }
        }

        public static string Notes
        {
            get { return "Notes"; }
            set { }
        }

        public static string Tasks
        {
            get { return "Tasks"; }
            set { }
        }

        public static string History
        {
            get { return "History"; }
            set { }
        }
    }
}