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
using ServerApp.Models.MongoViewModels.User;

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

        public HttpResponseMessage UserProfile(UserProfile postData, [ValueProvider(typeof(HeaderValueProviderFactory<string>))] string authKey)
        {
            HttpResponseMessage responseMessage;

            User sqlUser;
            if (!ValidateCredentials.AuthKeyIsValid(db, authKey, out sqlUser))
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid information.");
                return responseMessage;
            }
            //todo check if user is in organization
            var queriedOrganization = GenericQueries.CheckOrganizationName(postData.OrganizationName, mongoDb);
            if (queriedOrganization == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid organization name.");
                return responseMessage;
            }

            MongoCollection<UsersProjects> usersProjects = mongoDb.GetCollection<UsersProjects>(MongoCollections.UsersInProjects);

            var projectsInvolved = (from up in usersProjects.AsQueryable<UsersProjects>()
                                    where up.Username == sqlUser.Username && up.OrganizationName == postData.OrganizationName
                                    select up.ProjectName
                                    );

            return responseMessage = this.Request.CreateResponse(HttpStatusCode.OK, new { Projects = projectsInvolved });
        }

        public HttpResponseMessage UserAdminProfile(UserInProject postData, [ValueProvider(typeof(HeaderValueProviderFactory<string>))] string authKey)
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
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid organization name.");
                return responseMessage;
            }

            MongoCollection<UsersOrganizations> usersAndOrganizations = mongoDb.GetCollection<UsersOrganizations>(MongoCollections.UsersInOrganizations);
            UsersOrganizations userAssigning;
            GenericQueries.CheckUser(authKey, queriedOrganization, usersAndOrganizations, out userAssigning, UserRoles.OrganizationManager, db);
            if (userAssigning == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Insufficient permissions.");
                return responseMessage;
            }

            MongoCollection<Project> projectsCollection = mongoDb.GetCollection<Project>(MongoCollections.Projects);

            var allProjects = (from pr in projectsCollection.AsQueryable<Project>()
                               where pr.OrganizationName == postData.OrganizationName
                               select new ProjectForUser()
                               {
                                   Name = pr.Name,
                               }
                               ).ToDictionary<ProjectForUser, string>(x => x.Name);

            MongoCollection<UsersProjects> usersProjects = mongoDb.GetCollection<UsersProjects>(MongoCollections.UsersInProjects);

            var projectsInvolved = (from up in usersProjects.AsQueryable<UsersProjects>()
                                    where up.Username == postData.Username
                                    select up
                                    );

            foreach (var proj in projectsInvolved)
            {
                if (allProjects.ContainsKey(proj.ProjectName))
                {
                    var participating = allProjects[proj.ProjectName];
                    participating.UserParticipatesIn = true;
                    participating.Role = proj.Role;
                }
            }

            UsersOrganizations usersProfile = usersAndOrganizations.AsQueryable<UsersOrganizations>()
                .FirstOrDefault(x => x.Username == postData.Username && x.OrganizationName == postData.OrganizationName);

            return responseMessage = this.Request.CreateResponse(HttpStatusCode.OK, new { Projects = allProjects.Values, Role = usersProfile.Role });
        }

        public HttpResponseMessage SetProjectManager(SetAsProjectManager postData, [ValueProvider(typeof(HeaderValueProviderFactory<string>))] string authKey)
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
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid organization name.");
                return responseMessage;
            }

            MongoCollection<UsersOrganizations> usersAndOrganizations = mongoDb.GetCollection<UsersOrganizations>(MongoCollections.UsersInOrganizations);
            UsersOrganizations userAssigning;
            UsersOrganizations usersProfile = usersAndOrganizations.FindOneAs<UsersOrganizations>(Query.EQ("Username", postData.Username));
            GenericQueries.CheckUser(authKey, queriedOrganization, usersAndOrganizations, out userAssigning, UserRoles.OrganizationManager, db);

            if (usersProfile == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No such user in organization.");
                return responseMessage;
            }
            if (userAssigning == null || userAssigning.Role < usersProfile.Role)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Insufficient permissions.");
                return responseMessage;
            }

            MongoCollection<UsersProjects> usersProjects = mongoDb.GetCollection<UsersProjects>(MongoCollections.UsersInProjects);
            UsersProjects userInProject = usersProjects.FindOneAs<UsersProjects>(Query.And(
                Query.EQ("ProjectName", postData.ProjectName),
                Query.EQ("Username", postData.Username),
                Query.EQ("OrganizationName", postData.OrganizationName)));

            userInProject.Role = postData.SetAsManager ? UserRoles.ProjectManager : usersProfile.Role;
            usersProjects.Save(userInProject);
            HistoryController.RecordHistoryEntry(userAssigning.OrganizationName, userAssigning.Username, 
                string.Format("{0} {1} as Project Manager", postData.SetAsManager ? "assigned" : "dissociated", userInProject.Username), userInProject.ProjectName);

            return responseMessage = this.Request.CreateResponse(HttpStatusCode.OK, new { User = userInProject });
        }

        public HttpResponseMessage ChangeUserRole(UserProfile postData, [ValueProvider(typeof(HeaderValueProviderFactory<string>))] string authKey)
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
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid organization name.");
                return responseMessage;
            }

            MongoCollection<UsersOrganizations> usersAndOrganizations = mongoDb.GetCollection<UsersOrganizations>(MongoCollections.UsersInOrganizations);
            UsersOrganizations userAssigning;
            //UsersOrganizations usersProfile = usersAndOrganizations.FindOneAs<UsersOrganizations>(Query.EQ("Username", postData.Username));
            UsersOrganizations usersProfile = usersAndOrganizations.AsQueryable<UsersOrganizations>()
                .FirstOrDefault(x => x.Username == postData.Username && x.OrganizationName == postData.OrganizationName);
            GenericQueries.CheckUser(authKey, queriedOrganization, usersAndOrganizations, out userAssigning, UserRoles.OrganizationManager, db);

            if (usersProfile == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No such user in organization.");
                return responseMessage;
            }
            if (userAssigning == null || userAssigning.Role < usersProfile.Role)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Insufficient permissions.");
                return responseMessage;
            }


            HistoryController.RecordHistoryEntry(userAssigning.OrganizationName, userAssigning.Username,
                string.Format("{0} {1}", usersProfile.Role < postData.UserRole ? "promoted" : "demoted", usersProfile.Username));

            usersProfile.Role = postData.UserRole;
            usersAndOrganizations.Save(usersProfile);
            ChangeUserRoleInProjects(usersProfile);
            

            return responseMessage = this.Request.CreateResponse(HttpStatusCode.OK);
        }

        private void ChangeUserRoleInProjects(UsersOrganizations user)
        {
            MongoCollection<UsersProjects> usersProjects = mongoDb.GetCollection<UsersProjects>(MongoCollections.UsersInProjects);
            IMongoQuery query;
            if (user.Role < UserRoles.ProjectManager)
            {
                query = Query.And(
                    Query.NE("Role", UserRoles.ProjectManager),
                    Query.EQ("Username", user.Username)
                    );
            }
            else
            {
                query = Query.And(
                    Query.EQ("Username", user.Username)
                    );
            }

            var update = Update.Set("Role", user.Role);
            usersProjects.Update(query, update, UpdateFlags.Multi);
        }

        public HttpResponseMessage AllUsersInOrganization(UserInProject postData, [ValueProvider(typeof(HeaderValueProviderFactory<string>))] string authKey)
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
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid organization name.");
                return responseMessage;
            }

            MongoCollection<UsersOrganizations> usersAndOrganizations = mongoDb.GetCollection<UsersOrganizations>(MongoCollections.UsersInOrganizations);
            UsersOrganizations queryingUser;
            GenericQueries.CheckUser(authKey, queriedOrganization, usersAndOrganizations, out queryingUser, UserRoles.OrganizationManager, db);

            if (queryingUser == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Insufficient permissions.");
                return responseMessage;
            }


            var usersInOrganization = (from us in usersAndOrganizations.AsQueryable<UsersOrganizations>()
                                       where us.OrganizationId == queriedOrganization.Id
                                       select new UsersInOrganizationVM()
                                       {
                                           Role = us.Role,
                                           Username = us.Username
                                       }).ToList();


            Dictionary<string, List<UsersInOrganizationVM>> grouped = new Dictionary<string, List<UsersInOrganizationVM>>();

            foreach (var item in usersInOrganization)
            {
                string stringRole = ConvertRoleToString(item.Role);
                if (grouped.ContainsKey(stringRole))
                {
                    grouped[stringRole].Add(item);
                }
                else
                {
                    grouped.Add(stringRole, new List<UsersInOrganizationVM>());
                    grouped[stringRole].Add(item);
                }
            }

            return responseMessage = this.Request.CreateResponse(HttpStatusCode.OK,
                new { Keys = grouped.Keys, Users = grouped });

        }

        private string ConvertRoleToString(UserRoles userRole)
        {
            string role;
            switch (userRole)
            {
                case UserRoles.JuniorEmployee: role = "JuniorEmployee"; break;
                case UserRoles.Employee: role = "Employee"; break;
                case UserRoles.SeniorEmployee: role = "SeniorEmployee"; break;
                case UserRoles.ProjectManager: role = "ProjectManager"; break;
                case UserRoles.OrganizationManager: role = "OrganizationManager"; break;
                case UserRoles.OrganizationOwner: role = "OrganizationOwner"; break;
                default:
                    throw new ArgumentException("Invalid role");

            };

            return role;
        }

    }
}