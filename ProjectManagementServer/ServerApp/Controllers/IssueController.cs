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
using ServerApp.Models;

namespace ServerApp.Controllers
{
    public class IssueController : ApiController
    {
        IUoWData db;
        MongoDatabase mongoDb;

        public IssueController(IUoWData db)
        {
            this.db = db;
            this.mongoDb = MongoClientFactory.GetDatabase();
        }

        public HttpResponseMessage PostAnswerToIssue(AnswerIssue answer, [ValueProvider(typeof(HeaderValueProviderFactory<string>))] string authKey)
        {
            HttpResponseMessage responseMessage;
            User sqlUser;
            if (!ValidateCredentials.AuthKeyIsValid(db, authKey, out sqlUser))
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid information.");
                return responseMessage;
            }

            MongoCollection<UsersProjects> usersInProjects = mongoDb.GetCollection<UsersProjects>(MongoCollections.UsersInProjects);

            // todo projects need to be recognized by id
            // the relation table has to save the id and use it in further queries

            

            MongoCollection<OpenIssue> issuesCollection = mongoDb.GetCollection<OpenIssue>(MongoCollections.Issues);
            var issue = issuesCollection.AsQueryable<OpenIssue>().FirstOrDefault(x => x.Id == new ObjectId(answer.IssueId));
            if (issue == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No such issue.");
                return responseMessage;
            }

            issue.Answers.Add(answer);
            issuesCollection.Save(issue);

            List<AnswerIssue> entries = GetEntries(issue);

            return responseMessage = this.Request.CreateResponse(HttpStatusCode.OK, new { Entries = entries });
        }

        public HttpResponseMessage PostIssue(OpenIssue postedIssue, [ValueProvider(typeof(HeaderValueProviderFactory<string>))] string authKey)
        {
            HttpResponseMessage responseMessage;
            User sqlUser;
            if (!ValidateCredentials.AuthKeyIsValid(db, authKey, out sqlUser))
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid information.");
                return responseMessage;
            }

            MongoCollection<UsersProjects> usersInProjects = mongoDb.GetCollection<UsersProjects>(MongoCollections.UsersInProjects);

            // todo projects need to be recognized by id
            // the relation table has to save the id and use it in further queries

            UsersProjects postingUser = usersInProjects.AsQueryable<UsersProjects>()
                .FirstOrDefault(x => x.Username == sqlUser.Username
                && x.ProjectName == postedIssue.ProjectName);
            if (postingUser == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "User does not participate in project.");
                return responseMessage;
            }

            MongoCollection<OpenIssue> issuesCollection = mongoDb.GetCollection<OpenIssue>(MongoCollections.Issues);
            postedIssue.ByUser = sqlUser.Username;
            postedIssue.Answers = new List<AnswerIssue>();
            issuesCollection.Save(postedIssue);

            return responseMessage = this.Request.CreateResponse(HttpStatusCode.OK, new { Posted = "Success" });
        }

        [HttpPost]
        public HttpResponseMessage GetIssue(ProjectPostData postData, [ValueProvider(typeof(HeaderValueProviderFactory<string>))] string authKey)
        {
            // todo get issue needs only issue id
            HttpResponseMessage responseMessage;
            User sqlUser;
            if (!ValidateCredentials.AuthKeyIsValid(db, authKey, out sqlUser))
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid information.");
                return responseMessage;
            }

            MongoCollection<UsersProjects> usersInProjects = mongoDb.GetCollection<UsersProjects>(MongoCollections.UsersInProjects);

            // todo projects need to be recognized by id
            // the relation table has to save the id and use it in further queries

            UsersProjects postingUser = usersInProjects.AsQueryable<UsersProjects>()
                .FirstOrDefault(x => x.Username == sqlUser.Username
                && x.ProjectName == postData.ProjectName);
            if (postingUser == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "User does not participate in project.");
                return responseMessage;
            }

            MongoCollection<OpenIssue> issuesCollection = mongoDb.GetCollection<OpenIssue>(MongoCollections.Issues);
            var issue = issuesCollection.AsQueryable<OpenIssue>().FirstOrDefault(x => x.Id == new ObjectId(postData.IssueId));
            if (issue == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No such issue.");
                return responseMessage;
            }

            List<AnswerIssue> entries = GetEntries(issue);

            return responseMessage = this.Request.CreateResponse(HttpStatusCode.OK, new { Entries = entries, Title = issue.Title });
        }

        private static List<AnswerIssue> GetEntries(OpenIssue issue)
        {
            List<AnswerIssue> entries = new List<AnswerIssue>();
            AnswerIssue question = new AnswerIssue()
            {
                Time = issue.DatePosted,
                Username = issue.ByUser,
                Text = issue.Text
            };

            entries.Add(question);
            entries.AddRange(issue.Answers);
            return entries;
        }
    }
}