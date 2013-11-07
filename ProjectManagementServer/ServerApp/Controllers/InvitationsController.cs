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


namespace ServerApp.Controllers
{
    public class InvitationsController : ApiController
    {
        IUoWData db;
        MongoDatabase mongoDb;

        public InvitationsController(IUoWData db)
        {
            this.db = db;
            this.mongoDb = MongoClientFactory.GetDatabase();
        }

        [HttpPost]
        public HttpResponseMessage InviteToOrganization(Invitation invitation, [ValueProvider(typeof(HeaderValueProviderFactory<string>))] string authKey)
        {
            HttpResponseMessage responseMessage;

            if (!ValidateCredentials.AuthKeyIsValid(db, authKey))
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid information.");
                return responseMessage;
            }

            var queriedOrganization = GenericQueries.CheckOrganizationName(invitation.OrganizationName, mongoDb);
            if (queriedOrganization == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid organization name.");
                return responseMessage;
            }

            MongoCollection<BsonDocument> usersAndOrganizations = mongoDb.GetCollection(MongoCollections.UsersInOrganizations);
            UsersOrganizations invitator;
            GenericQueries.CheckUser(authKey, queriedOrganization, usersAndOrganizations, out invitator, UserRoles.OrganizationManager, db);

            if (invitator == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Insufficient permissions.");
                return responseMessage;
            }

            MongoCollection users = mongoDb.GetCollection(MongoCollections.Users);
            User invited;
            GenericQueries.FindPersonalProfile(out invited, users, invitation.InvitedUser.ToLower());
            if (invited == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "User does not exist");
                return responseMessage;
            }

            var invitationsCollection = mongoDb.GetCollection(MongoCollections.Invitations);
            //todo check if invitation already exists
            invitation.InvitedUserId = invited._MongoId;
            //username or email is unneeded
            invitation.InvitedUser = "";
            invitationsCollection.Save(invitation);


            return responseMessage = this.Request.CreateResponse(HttpStatusCode.OK,
                new { Success = "Invitation sent." });
        }

       
    }
}