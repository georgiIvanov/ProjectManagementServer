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
    public class NoteController : ApiController
    {
        IUoWData db;
        MongoDatabase mongoDb;

        public NoteController(IUoWData db)
        {
            this.db = db;
            this.mongoDb = MongoClientFactory.GetDatabase();
        }

        public HttpResponseMessage PostNote(Note postedNote, [ValueProvider(typeof(HeaderValueProviderFactory<string>))] string authKey)
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
                && x.ProjectName == postedNote.ProjectName && x.Role >= UserRoles.SeniorEmployee);
            if (postingUser == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "User does not participate in project or role is insufficient.");
                return responseMessage;
            }

            MongoCollection<Note> notesCollection = mongoDb.GetCollection<Note>(MongoCollections.Notes);

            notesCollection.Save(postedNote);

            return responseMessage = this.Request.CreateResponse(HttpStatusCode.OK, new { Posted = "Success" });
        }
    }
       
}