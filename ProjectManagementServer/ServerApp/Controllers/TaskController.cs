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
    public class TaskController : ApiController
    {
        IUoWData db;
        MongoDatabase mongoDb;

        public TaskController(IUoWData db)
        {
            this.db = db;
            this.mongoDb = MongoClientFactory.GetDatabase();
        }

        public HttpResponseMessage AllTasksForProject(UserInProject user, [ValueProvider(typeof(HeaderValueProviderFactory<string>))] string authKey)
        {
            HttpResponseMessage responseMessage;
            User sqlUser;
            if (!ValidateCredentials.AuthKeyIsValid(db, authKey, out sqlUser))
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid information.");
                return responseMessage;
            }

            var usersProjects = mongoDb.GetCollection<UsersProjects>(MongoCollections.UsersInProjects);
            UsersProjects userInProject = usersProjects.FindOneAs<UsersProjects>(Query.And(
               Query.EQ("ProjectName", user.ProjectName),
               Query.EQ("Username", sqlUser.Username),
               Query.EQ("OrganizationName", user.OrganizationName)));
            if (userInProject == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No such organization, project, or user does not participate in them.");
                return responseMessage;
            }

            var tasksCollection = mongoDb.GetCollection<ProjectTask>(MongoCollections.Tasks);

            var openTasks = tasksCollection.AsQueryable<ProjectTask>().Where(x => x.ProjectName == user.ProjectName && x.Completed == false);
            var completedTasks = tasksCollection.AsQueryable<ProjectTask>().Where(x => x.ProjectName == user.ProjectName && x.Completed == true);

            return responseMessage = this.Request.CreateResponse(HttpStatusCode.OK, new { Opentasks = openTasks, CompletedTasks = completedTasks });
        }

        

        public HttpResponseMessage CreateTask(ProjectTask task, [ValueProvider(typeof(HeaderValueProviderFactory<string>))] string authKey)
        {
            HttpResponseMessage responseMessage;
            User sqlUser;
            if (!ValidateCredentials.AuthKeyIsValid(db, authKey, out sqlUser) || 
                task.TaskName.Length < 1 || task.TaskName.Length > 20 || task.TaskDescription.Length > 100)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid information.");
                return responseMessage;
            }

            var usersProjects = mongoDb.GetCollection<UsersProjects>(MongoCollections.UsersInProjects);
            UsersProjects userInProject = usersProjects.FindOneAs<UsersProjects>(Query.And(
               Query.EQ("ProjectName", task.ProjectName),
               Query.EQ("Username", sqlUser.Username),
               Query.EQ("OrganizationName", task.OrganizationName)));
            if (userInProject == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No such organization, project, or user does not participate in them.");
                return responseMessage;
            }
            if (userInProject.Role < UserRoles.ProjectManager)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Insufficient permissions.");
                return responseMessage;
            }

            var tasksCollection = mongoDb.GetCollection<ProjectTask>(MongoCollections.Tasks);
            task.UsersParticipating = new List<string>();
            tasksCollection.Save(task);

            return responseMessage = this.Request.CreateResponse(HttpStatusCode.OK, new { Posted = "Success" });
        }
    }
}