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
        public HttpResponseMessage RejectInvitation(InvitationViewModel rejectedInvitaion, [ValueProvider(typeof(HeaderValueProviderFactory<string>))] string authKey)
        {
            HttpResponseMessage responseMessage;

            if (!ValidateCredentials.AuthKeyIsValid(db, authKey))
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid information.");
                return responseMessage;
            }

            var user = db.Users.All().Single(x => x.AuthKey == authKey);

            var invitations = mongoDb.GetCollection(MongoCollections.Invitations);

            var foundInvitation = FindInvitation(rejectedInvitaion, user, invitations);
            if (foundInvitation == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid invitation.");
                return responseMessage;
            }

            invitations.Remove(Query.EQ("_id", foundInvitation.Id));

            return responseMessage = this.Request.CreateResponse(HttpStatusCode.OK,
                new { Removed = "Success" });
        }

        

        [HttpPost]
        public HttpResponseMessage AcceptInvitation(InvitationViewModel acceptedInvitaion, [ValueProvider(typeof(HeaderValueProviderFactory<string>))] string authKey)
        {
            HttpResponseMessage responseMessage;

            if (!ValidateCredentials.AuthKeyIsValid(db, authKey))
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid information.");
                return responseMessage;
            }

            var user = db.Users.All().Single(x => x.AuthKey == authKey);

            var invitations = mongoDb.GetCollection(MongoCollections.Invitations);

            var foundInvitation = FindInvitation(acceptedInvitaion, user, invitations);
            if (foundInvitation == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid invitation.");
                return responseMessage;
            }

            var organizations = mongoDb.GetCollection(MongoCollections.Organizations);

            var organization = organizations.FindOneAs<Organization>(Query.EQ("Name", foundInvitation.OrganizationName));

            CreateUserOrganizationRelation(organization, user, UserRoles.JuniorEmployee, mongoDb);

            invitations.Remove(Query.EQ("_id", foundInvitation.Id));

            return responseMessage = this.Request.CreateResponse(HttpStatusCode.OK,
                new { Accepted = "Success" });
        }

        [HttpGet]
        public HttpResponseMessage CheckForInvitations([ValueProvider(typeof(HeaderValueProviderFactory<string>))] string authKey)
        {
            HttpResponseMessage responseMessage;

            if (!ValidateCredentials.AuthKeyIsValid(db, authKey))
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid information.");
                return responseMessage;
            }

            var user = db.Users.All().Single(x => x.AuthKey == authKey);

            var invitations = mongoDb.GetCollection(MongoCollections.Invitations);

            var foundInvitations = invitations.AsQueryable<Invitation>()
                .Where(x => x.InvitedUserId == new ObjectId(user.MongoId)).Count();

            return responseMessage = this.Request.CreateResponse(HttpStatusCode.OK,
                new { InvitationsCount = foundInvitations.ToString() });
        }
        
        [HttpGet]
        public HttpResponseMessage SeeInvitations([ValueProvider(typeof(HeaderValueProviderFactory<string>))] string authKey)
        {
            HttpResponseMessage responseMessage;

            if (!ValidateCredentials.AuthKeyIsValid(db, authKey))
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid information.");
                return responseMessage;
            }

            var user = db.Users.All().Single(x => x.AuthKey == authKey);

            var invitations = mongoDb.GetCollection(MongoCollections.Invitations);

            var foundInvitations = invitations.AsQueryable<Invitation>()
                .Where(x => x.InvitedUserId == new ObjectId(user.MongoId))
                .Select(x => new InvitationViewModel()
                {
                    Id = x.Id.ToString(),
                    OrganizationName = x.OrganizationName
                });

            return responseMessage = this.Request.CreateResponse(HttpStatusCode.OK,
                new { Invitations = foundInvitations });
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
            invitationsCollection.Save(invitation);


            return responseMessage = this.Request.CreateResponse(HttpStatusCode.OK,
                new { Success = "Invitation sent." });
        }

        private void CreateUserOrganizationRelation(Organization organization, User sqlUser, UserRoles role, MongoDatabase mongoDb)
        {
            var usersCollection = mongoDb.GetCollection(MongoCollections.Users);

            var mongoUser = usersCollection.FindOne(Query.EQ("_id", new ObjectId(sqlUser.MongoId)));

            var usersOrganizations = mongoDb.GetCollection(MongoCollections.UsersInOrganizations);

            UsersOrganizations newRelation = new UsersOrganizations()
            {
                UserId = mongoUser["_id"].AsObjectId,
                OrganizationId = organization.Id,
                Name = organization.Name,
                Username = sqlUser.Username,
                Role = role
            };

            usersOrganizations.Save(newRelation);
        }

        private static Invitation FindInvitation(InvitationViewModel invitaion, Server.Models.User user, MongoCollection<BsonDocument> invitations)
        {
            var foundInvitation = invitations.AsQueryable<Invitation>()
                .FirstOrDefault(x => x.InvitedUserId == new ObjectId(user.MongoId) &&
                    x.Id == new ObjectId(invitaion.Id));
            return foundInvitation;
        }
    }
}