using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Server.Data;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Server.Models.MongoDbModels;
using System.Web.Http.ValueProviders;
using ServerApp.Utilities;

namespace ServerApp.Controllers
{
    public class OrganizationController : ApiController
    {
        IUoWData db;
        MongoDatabase mongoDb;
        
        public OrganizationController(IUoWData db)
        {
            this.db = db;
            this.mongoDb = MongoClientFactory.GetDatabase();
        }

        public HttpResponseMessage CreateOrganization(Organization organization, [ValueProvider(typeof(HeaderValueProviderFactory<string>))] string authKey)
        {
            HttpResponseMessage responseMessage;

            if (!ModelState.IsValid)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid information.");
                return responseMessage;
            }

            var organizationsCollection = mongoDb.GetCollection("Organizations");

            var alreadyExists = organizationsCollection.FindAs<Organization>(Query.EQ("Name", organization.Name)).Count();
            if (alreadyExists > 0)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Organization with same name already exists");
                return responseMessage;
            }

            organization.DateCreated = DateTime.Now;
            organization.ProjectsIdsInOrganization = new BsonDocument();
            organization.UsersIdsInOrganization = new BsonDocument();

            organizationsCollection.Save<Organization>(organization);

            organization.ProjectsIdsInOrganization = null;
            organization.UsersIdsInOrganization = null;

            CreateUserOrganizationRelation(organization, authKey);

            responseMessage = this.Request.CreateResponse(HttpStatusCode.OK, organization);

            return responseMessage; 
        }

        private void CreateUserOrganizationRelation(Organization organization, string authKey)
        {
            var sqlUser = db.Users.All().Single(x => x.AuthKey == authKey);
            var usersCollection = mongoDb.GetCollection("Users");

            var mongoUser = usersCollection.FindOne(Query.EQ("_id", new ObjectId(sqlUser.MongoId)));

            var usersOrganizations = mongoDb.GetCollection("UsersInOrganizations");

            UsersOrganizations newRelation = new UsersOrganizations()
            {
                UserId = mongoUser["_id"].AsObjectId,
                OrganizationId = organization.Id
            };

            usersOrganizations.Save(newRelation);
        }

    }
}