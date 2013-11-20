using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using Server.Data;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Server.Models.MongoDbModels;
using System.Web.Http.ValueProviders;
using ServerApp.Utilities;
using System.Collections.Generic;
using ServerApp.Models.MongoViewModels;
using Server.Models;
using System.Text;
using ServerApp.Models.MongoViewModels.Project;

namespace ServerApp.Controllers
{
    static class HistoryController
    {
        static MongoDatabase mongoDb;
        static MongoCollection<HistoryEntry> historyCollection;

        static HistoryController()
        {
            mongoDb = MongoClientFactory.GetDatabase();
            historyCollection = mongoDb.GetCollection<HistoryEntry>(MongoCollections.History);
        }

        public static void RecordHistoryEntry(string organizationName, string byUser, string additionalInfo = "", string projectName = "")
        {
            HistoryEntry entry = new HistoryEntry()
            {
                ProjectName = projectName,
                ByUser = byUser,
                Informaiton = additionalInfo,
                OrganizationName = organizationName,
                TimeRecorded = DateTime.Now.Date
            };

            historyCollection.Save(entry);
        }

        public static string GetEventsForOrganization(string organizationName)
        {
            var entries = historyCollection.FindAs<HistoryEntry>(Query.EQ("OrganizationName", organizationName))
                .SetSortOrder(SortBy.Ascending("TimeRecorded"));
            StringBuilder sb = new StringBuilder();
            foreach (var item in entries)
            {
                if (string.IsNullOrEmpty(item.ProjectName))
                {
                    sb.AppendFormat("By {1} - {2}, at {3}", item.ProjectName, item.ByUser, item.Informaiton, item.TimeRecorded.ToShortDateString());
                }
                else
                {
                    sb.AppendFormat("In {0}, by {1} - {2}, at {3}", item.ProjectName, item.ByUser, item.Informaiton, item.TimeRecorded.ToShortDateString());
                }
                sb.AppendLine();
                sb.AppendLine(" - ");
            }

            return sb.ToString();
        }

        
    }
}