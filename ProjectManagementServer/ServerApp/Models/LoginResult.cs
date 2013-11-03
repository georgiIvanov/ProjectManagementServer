using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServerApp.Models
{
    public class LoginResult
    {
        public string AuthKey { get; set; }
        public DateTime LastLogged { get; set; }
    }
}