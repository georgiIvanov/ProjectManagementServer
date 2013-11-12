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
    public class UserController : ApiController
    {
        IUoWData db;
        MongoDatabase mongoDb;

        public UserController(IUoWData db)
        {
            this.db = db;
            this.mongoDb = MongoClientFactory.GetDatabase();
        }

        public HttpResponseMessage GetAllUsersInOrganization(string organizationName, [ValueProvider(typeof(HeaderValueProviderFactory<string>))] string authKey)
        {
            HttpResponseMessage responseMessage;

            if (!ValidateCredentials.AuthKeyIsValid(db, authKey))
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid information.");
                return responseMessage;
            }
            var queriedOrganization = GenericQueries.CheckOrganizationName(organizationName, mongoDb);
            if (queriedOrganization == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid organization name.");
                return responseMessage;
            }

            MongoCollection<BsonDocument> usersAndOrganizations = mongoDb.GetCollection(MongoCollections.UsersInOrganizations);
            UsersOrganizations queryingUser;
            GenericQueries.CheckUser(authKey, queriedOrganization, usersAndOrganizations, out queryingUser, UserRoles.OrganizationManager, db);

            if (queryingUser == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Insufficient permissions.");
                return responseMessage;
            }

            //var usersInOrganization = usersAndOrganizations.Find(Query.EQ("_id", queriedOrganization.Id)).ToList();

            var usersInOrganization = (from us in usersAndOrganizations.AsQueryable<UsersOrganizations>()
                                       where us.OrganizationId == queriedOrganization.Id
                                       select new UsersInOrganizationVM()
                                       {
                                           Role = us.Role,
                                           Username = us.Username
                                       });

            //    .Select(x => new UsersInOrganizationVM()
            //{
            //    Role = (UserRoles)x.GetValue("Role").AsInt32
            //});

            //var returned = (from us in usersAndOrganizations
            //                select new UsersInOrganizationVM()
            //                {
            //                    Username = us.Username,
            //                    Role = us.Role
            //                });
            

            return responseMessage = this.Request.CreateResponse(HttpStatusCode.OK,
                new { Users = usersInOrganization });

        }
    }
}