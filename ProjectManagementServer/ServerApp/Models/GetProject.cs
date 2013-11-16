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
    public class ProjectPostData
    {
        public string ProjectName { get; set; }
        public string IssueId { get; set; }
    }
}