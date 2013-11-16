using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServerApp.Models
{
    /// <summary>
    /// added because project with spaces does not 
    /// create a proper url in iOS
    /// </summary>
    public class GetProject
    {
        public string ProjectName { get; set; }
    }
}