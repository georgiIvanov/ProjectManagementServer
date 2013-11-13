using Server.Data;
using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServerApp.Utilities
{
    public static class ValidateCredentials
    {
        public static bool AuthKeyIsValid(IUoWData db, string authKey)
        {
            var found = db.Users.All().FirstOrDefault(x => x.AuthKey == authKey);
            if (found == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool AuthKeyIsValid(IUoWData db, string authKey, out User sqlUser)
        {
            var found = db.Users.All().FirstOrDefault(x => x.AuthKey == authKey);
            if (found == null)
            {
                sqlUser = null;
                return false;
            }
            else
            {
                sqlUser = found;
                return true;
            }
        }
    }
}