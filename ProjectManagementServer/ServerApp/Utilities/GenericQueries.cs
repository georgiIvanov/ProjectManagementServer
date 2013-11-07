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

namespace ServerApp.Utilities
{
    internal static class GenericQueries
    {
        internal static void FindPersonalProfile(out User user, MongoCollection usersCollection, string invitedUser)
        {
            user = usersCollection.AsQueryable<User>().FirstOrDefault(x => x.Email.ToLower() == invitedUser || x.Username.ToLower() == invitedUser);
        }

        internal static void CheckUser(string authKey, Organization queriedOrganization, MongoCollection<BsonDocument> usersAndOrganizations, out UsersOrganizations foundUser, UserRoles role, IUoWData db)
        {
            var userMongoId = db.Users.All().Single(x => x.AuthKey == authKey).MongoId;

            foundUser = usersAndOrganizations.AsQueryable<UsersOrganizations>()
.FirstOrDefault(x => x.Role >= role &&
    x.UserId == new ObjectId(userMongoId) && x.Name == queriedOrganization.Name);
        }

        internal static Organization CheckOrganizationName(string organizationName, MongoDatabase mongoDb)
        {
            var organizations = mongoDb.GetCollection(MongoCollections.Organizations);
            var queriedOrganization = organizations.AsQueryable<Organization>()
                                        .FirstOrDefault(x => x.Name == organizationName);
            return queriedOrganization;
        }
    }
}