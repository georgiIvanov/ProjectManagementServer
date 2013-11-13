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
                               select new ProjectForUser()
                               {
                                   Name = pr.Name
                               }
                               ).ToDictionary<ProjectForUser, string>(x => x.Name);

            MongoCollection<UsersProjects> usersProjects = mongoDb.GetCollection<UsersProjects>(MongoCollections.UsersInProjects);

            var projectsInvolved = (from up in usersProjects.AsQueryable<UsersProjects>()
                                    where up.Username == postData.Username
                                    select up.ProjectName
                                    );

            foreach (var projName in projectsInvolved)
            {
                if (allProjects.ContainsKey(projName))
                {
                    allProjects[projName].UserParticipatesIn = true;
                }
            }

            return responseMessage = this.Request.CreateResponse(HttpStatusCode.OK, new { Projects = allProjects.Values });
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


            Dictionary<string,List<UsersInOrganizationVM>> grouped = new Dictionary<string, List<UsersInOrganizationVM>>();

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
            switch(userRole)
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