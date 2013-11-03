using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Server.Data;
using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json;

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

        public HttpResponseMessage CreateOrganization(Organization organization)
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
            //this.ControllerContext.Configuration.
            organization.ProjectsIdsInOrganization = null;
            organization.UsersIdsInOrganization = null;
            responseMessage = this.Request.CreateResponse(HttpStatusCode.OK, organization);

            return responseMessage; 
        }

    }
}