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

namespace ServerApp.Controllers
{
    public class ProjectController : ApiController
    {
        IUoWData db;
        MongoDatabase mongoDb;

        public ProjectController(IUoWData db)
        {
            this.db = db;
            this.mongoDb = MongoClientFactory.GetDatabase();
        }

        public HttpResponseMessage CreateProject(Project project, [ValueProvider(typeof(HeaderValueProviderFactory<string>))] string authKey)
        {
            HttpResponseMessage responseMessage;

            if (!ValidateCredentials.AuthKeyIsValid(db, authKey))
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid information.");
                return responseMessage;
            }
            var queriedOrganization = GenericQueries.CheckOrganizationName(project.OrganizationName, mongoDb);
            if (queriedOrganization == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid organization name.");
                return responseMessage;
            }

            MongoCollection<BsonDocument> usersAndOrganizations = mongoDb.GetCollection(MongoCollections.UsersInOrganizations);
            UsersOrganizations userCreating;
            GenericQueries.CheckUser(authKey, queriedOrganization, usersAndOrganizations, out userCreating, UserRoles.OrganizationManager, db);

            if (userCreating == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Insufficient permissions.");
                return responseMessage;
            }

            MongoCollection<BsonDocument> projects = mongoDb.GetCollection(MongoCollections.Projects);
            projects.Save(project);

            return responseMessage = this.Request.CreateResponse(HttpStatusCode.OK,
                new { Created = "Success" });
        }
    }
}