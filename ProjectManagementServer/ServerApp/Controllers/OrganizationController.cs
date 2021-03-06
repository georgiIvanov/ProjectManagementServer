﻿using MongoDB.Bson;
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

        public HttpResponseMessage RecentEvents(UserInProject postData, [ValueProvider(typeof(HeaderValueProviderFactory<string>))] string authKey)
        {
            HttpResponseMessage responseMessage;

            if (!ValidateCredentials.AuthKeyIsValid(db, authKey))
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid information.");
                return responseMessage;
            }

            var queriedOrganization = GenericQueries.CheckOrganizationName(postData.OrganizationName, mongoDb);
            if (queriedOrganization == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid name.");
                return responseMessage;
            }

            MongoCollection<UsersOrganizations> usersAndOrganizations = mongoDb.GetCollection<UsersOrganizations>(MongoCollections.UsersInOrganizations);
            UsersOrganizations foundUser;
            GenericQueries.CheckUser(authKey, queriedOrganization, usersAndOrganizations, out foundUser, UserRoles.OrganizationManager, db);

            if (foundUser == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Insufficient permissions.");
                return responseMessage;
            }

            string recentEvents = HistoryController.GetEventsForOrganization(postData.OrganizationName);

            return responseMessage = this.Request.CreateResponse(HttpStatusCode.OK,
            new { Events = recentEvents.ToString()});
        }

        public HttpResponseMessage FullInfo(UserInProject postData, [ValueProvider(typeof(HeaderValueProviderFactory<string>))] string authKey)
        {
            HttpResponseMessage responseMessage;

            if (!ValidateCredentials.AuthKeyIsValid(db, authKey))
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid information.");
                return responseMessage;
            }

            var queriedOrganization = GenericQueries.CheckOrganizationName(postData.OrganizationName, mongoDb);
            if (queriedOrganization == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid name.");
                return responseMessage;
            }

            MongoCollection<UsersOrganizations> usersAndOrganizations = mongoDb.GetCollection<UsersOrganizations>(MongoCollections.UsersInOrganizations);

            UsersOrganizations foundUser;
            GenericQueries.CheckUser(authKey, queriedOrganization, usersAndOrganizations, out foundUser, UserRoles.OrganizationManager, db);

            if (foundUser == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Insufficient permissions.");
                return responseMessage;
            }

            var projectsCollection = mongoDb.GetCollection(MongoCollections.Projects);

            int projectsInOrganization = projectsCollection.AsQueryable<Project>()
                                        .Count(x => x.OrganizationName == queriedOrganization.Name);
            int employeesCount = usersAndOrganizations.AsQueryable<UsersOrganizations>()
                                .Count(x => x.OrganizationId == queriedOrganization.Id);

            return responseMessage = this.Request.CreateResponse(HttpStatusCode.OK,
            new
            {
                Employees = employeesCount.ToString(),
                Projects = projectsInOrganization.ToString()
            });
        }

       

        [HttpGet]
        public HttpResponseMessage GetInvolvedOrganizations([ValueProvider(typeof(HeaderValueProviderFactory<string>))] string authKey)
        {
            HttpResponseMessage responseMessage;

            if (!ValidateCredentials.AuthKeyIsValid(db, authKey))
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid information.");
                return responseMessage;
            }

            var userMongoId = db.Users.All().Single(x => x.AuthKey == authKey).MongoId;


            var usersAndOrganizations = mongoDb.GetCollection(MongoCollections.UsersInOrganizations);

            List<OgranizationListEntry> found = (from o in usersAndOrganizations.AsQueryable<UsersOrganizations>()
                                                 where o.UserId == new ObjectId(userMongoId)
                                                 select new OgranizationListEntry()
                                                 {
                                                     Name = o.OrganizationName,
                                                     OrganizationId = o.OrganizationId,
                                                     Role = o.Role
                                                 }).ToList();

            return responseMessage = this.Request.CreateResponse(
                HttpStatusCode.OK, new { Organizations = found });
        }

        public HttpResponseMessage CreateOrganization(Organization organization, [ValueProvider(typeof(HeaderValueProviderFactory<string>))] string authKey)
        {
            HttpResponseMessage responseMessage;

            if (!ModelState.IsValid || !ValidateCredentials.AuthKeyIsValid(db, authKey))
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid information.");
                return responseMessage;
            }

            var organizationsCollection = mongoDb.GetCollection(MongoCollections.Organizations);

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

            GenericQueries.CreateUserOrganizationRelation(organization, authKey, UserRoles.OrganizationOwner, db, mongoDb);

            responseMessage = this.Request.CreateResponse(HttpStatusCode.OK, organization);

            return responseMessage;
        }

        

    }
}