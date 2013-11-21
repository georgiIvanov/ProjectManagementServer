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
using ServerApp.Models.MongoViewModels.Project;
using ServerApp.Models;

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

        [HttpPost]
        public HttpResponseMessage GetProjectInformation(ProjectPostData project, [ValueProvider(typeof(HeaderValueProviderFactory<string>))] string authKey)
        {
            HttpResponseMessage responseMessage;
            User sqlUser;
            if (!ValidateCredentials.AuthKeyIsValid(db, authKey, out sqlUser))
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid information.");
                return responseMessage;
            }

            MongoCollection<UsersProjects> usersInProjects = mongoDb.GetCollection<UsersProjects>(MongoCollections.UsersInProjects);

            // todo projects need to be recognized by id
            // the relation table has to save the id and use it in further queries

            UsersProjects postingUser = usersInProjects.AsQueryable<UsersProjects>()
                .FirstOrDefault(x => x.Username == sqlUser.Username
                && x.ProjectName == project.ProjectName);
            if (postingUser == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "User does not participate in project.");
                return responseMessage;
            }

            MongoCollection<OpenIssue> issuesCollection = mongoDb.GetCollection<OpenIssue>(MongoCollections.Issues);
            MongoCollection<Note> notesCollection = mongoDb.GetCollection<Note>(MongoCollections.Notes);
            MongoCollection<ProjectTask> tasksCollection = mongoDb.GetCollection<ProjectTask>(MongoCollections.Tasks);

            var issues = (from i in issuesCollection.AsQueryable<OpenIssue>()
                          where i.ProjectName == project.ProjectName
                         select new TableCell()
                         {
                             Id = i.Id.ToString(),
                             Title = i.Title
                         }).ToList();
            issues.Reverse();

            var notes = (from n in notesCollection.AsQueryable<Note>()
                         where n.ProjectName == project.ProjectName
                         select new TableCell()
                         {
                             Id = n.Id.ToString(),
                             Title = n.Title
                         }).ToList();

            var allTasksCount = tasksCollection.AsQueryable<ProjectTask>()
                .Where(x => x.ProjectName == project.ProjectName).Count();
            var allOpenTasks = tasksCollection.AsQueryable<ProjectTask>()
                .Where(x => x.Completed == false && x.ProjectName == project.ProjectName).Count();
            TableCell task = new TableCell()
            {
                Title = string.Format("All: {0}, Open: {1}", allTasksCount, allOpenTasks)
            };


            return responseMessage = this.Request.CreateResponse(HttpStatusCode.OK, 
                new { Issues = issues, Notes = notes, UserRoleInProject = postingUser.Role,
                        Tasks = task
                    });
        }

        public HttpResponseMessage PartInProject(UserInProject postData, [ValueProvider(typeof(HeaderValueProviderFactory<string>))] string authKey)
        {
            HttpResponseMessage responseMessage;
            User sqlUser;
            if (!ValidateCredentials.AuthKeyIsValid(db, authKey, out sqlUser))
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

            UsersOrganizations userAssigned = usersAndOrganizations.FindOneAs<UsersOrganizations>(Query.EQ("Username", postData.Username));
            if (userAssigned == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No such user in organization.");
                return responseMessage;
            }

            //todo check if assigned user isnt already in project
            MongoCollection<Project> projectsCollection = mongoDb.GetCollection<Project>(MongoCollections.Projects);
            Project project;
            project = projectsCollection.FindOneAs<Project>(Query.EQ("Name", postData.ProjectName));
            if (project == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid project.");
                return responseMessage;
            }
            MongoCollection<UsersProjects> usersProjects = mongoDb.GetCollection<UsersProjects>(MongoCollections.UsersInProjects);
            UsersProjects possibleRelation = usersProjects.AsQueryable<UsersProjects>()
                .FirstOrDefault(x => x.ProjectName == project.Name &&
                    x.Username == project.Name &&
                    x.OrganizationName == queriedOrganization.Name);
            if (possibleRelation != null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "User already in project.");
                return responseMessage;
            }

            CreateUserProjectRelation(userAssigned, project);
            HistoryController.RecordHistoryEntry(userAssigning.OrganizationName, userAssigning.Username,
                string.Format("assigned {0} in {1}", userAssigned.Username, project.Name));

            return responseMessage = this.Request.CreateResponse(HttpStatusCode.OK, new { Assigned = "Success" });
        }

        public HttpResponseMessage RemoveFromProject(UserInProject postData, [ValueProvider(typeof(HeaderValueProviderFactory<string>))] string authKey)
        {
            HttpResponseMessage responseMessage;
            User sqlUser;
            if (!ValidateCredentials.AuthKeyIsValid(db, authKey, out sqlUser))
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

            UsersOrganizations userRemoved = usersAndOrganizations.FindOneAs<UsersOrganizations>(Query.EQ("Username", postData.Username));
            if (userRemoved == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No such user in organization.");
                return responseMessage;
            }

            //todo check if assigned user isnt already in project
            MongoCollection<Project> projectsCollection = mongoDb.GetCollection<Project>(MongoCollections.Projects);
            Project project;
            project = projectsCollection.FindOneAs<Project>(Query.EQ("Name", postData.ProjectName));
            if (project == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid project.");
                return responseMessage;
            }

            MongoCollection<UsersProjects> usersProjects = mongoDb.GetCollection<UsersProjects>(MongoCollections.UsersInProjects);

            usersProjects.Remove(Query.EQ("Username", userRemoved.Username), RemoveFlags.Single);
            HistoryController.RecordHistoryEntry(userAssigning.OrganizationName, userAssigning.Username,
                string.Format("removed {0} from {1}", userRemoved.Username, project.Name));

            return responseMessage = this.Request.CreateResponse(HttpStatusCode.OK, new { Removed = "Success" });
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

            MongoCollection<UsersOrganizations> usersAndOrganizations = mongoDb.GetCollection<UsersOrganizations>(MongoCollections.UsersInOrganizations);
            UsersOrganizations userCreating;
            GenericQueries.CheckUser(authKey, queriedOrganization, usersAndOrganizations, out userCreating, UserRoles.OrganizationManager, db);

            if (userCreating == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Insufficient permissions.");
                return responseMessage;
            }
            //todo check if project name is unique
            MongoCollection<BsonDocument> projects = mongoDb.GetCollection(MongoCollections.Projects);
            projects.Save(project);

            CreateUserProjectRelation(userCreating, project);
            HistoryController.RecordHistoryEntry(queriedOrganization.Name, userCreating.Username, string.Format("created project - {0}", project.Name));

            return responseMessage = this.Request.CreateResponse(HttpStatusCode.OK,
                new { Created = "Success" });
        }

        private void CreateUserProjectRelation(UsersOrganizations userAssigned, Project project)
        {
            MongoCollection<UsersProjects> usersProjects = mongoDb.GetCollection<UsersProjects>(MongoCollections.UsersInProjects);
            UsersProjects usersProjectsRelation = new UsersProjects()
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                //UserId = userAssigned.Id,
                Username = userAssigned.Username,
                OrganizationName = project.OrganizationName,
                Role = userAssigned.Role
            };
            usersProjects.Save(usersProjectsRelation);
        }

        //private UserRoles SetRoleForProject(UserRoles usersRole)
        //{
        //    if (usersRole >= UserRoles.OrganizationManager)
        //    {
        //        return usersRole;
        //    }

        //    return usersRole;
        //}
    }
}