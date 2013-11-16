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
using ServerApp.Models.MongoViewModels.Project;
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

        [HttpPut]
        public HttpResponseMessage EditNote(EditNote editNote, [ValueProvider(typeof(HeaderValueProviderFactory<string>))] string authKey)
        {
            HttpResponseMessage responseMessage;
            User sqlUser;
            if (!ValidateCredentials.AuthKeyIsValid(db, authKey, out sqlUser))
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid information.");
                return responseMessage;
            }

            MongoCollection<Note> notesCollection = mongoDb.GetCollection<Note>(MongoCollections.Notes);
            Note foundNote = notesCollection.FindOneAs<Note>(Query.EQ("_id", new ObjectId(editNote.Id)));

            MongoCollection<UsersProjects> usersInProjects = mongoDb.GetCollection<UsersProjects>(MongoCollections.UsersInProjects);
            // todo projects need to be recognized by id
            UsersProjects postingUser = usersInProjects.AsQueryable<UsersProjects>()
                .FirstOrDefault(x => x.Username == sqlUser.Username
                && x.ProjectName == foundNote.ProjectName);
            if (postingUser == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "User does not participate in project.");
                return responseMessage;
            }

            foundNote.Text = editNote.Text;
            notesCollection.Save(foundNote);

            return responseMessage = this.Request.CreateResponse(HttpStatusCode.OK, new { Note = foundNote });
        }


        public HttpResponseMessage GetNote(string noteId, [ValueProvider(typeof(HeaderValueProviderFactory<string>))] string authKey)
        {
            HttpResponseMessage responseMessage;
            User sqlUser;
            if (!ValidateCredentials.AuthKeyIsValid(db, authKey, out sqlUser))
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid information.");
                return responseMessage;
            }

            MongoCollection<Note> notesCollection = mongoDb.GetCollection<Note>(MongoCollections.Notes);
            Note foundNote = notesCollection.FindOneAs<Note>(Query.EQ("_id", new ObjectId(noteId)));

            MongoCollection<UsersProjects> usersInProjects = mongoDb.GetCollection<UsersProjects>(MongoCollections.UsersInProjects);

            // todo projects need to be recognized by id
            UsersProjects postingUser = usersInProjects.AsQueryable<UsersProjects>()
                .FirstOrDefault(x => x.Username == sqlUser.Username
                && x.ProjectName == foundNote.ProjectName);
            if (postingUser == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "User does not participate in project.");
                return responseMessage;
            }

            return responseMessage = this.Request.CreateResponse(HttpStatusCode.OK, new { Note = foundNote });
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
                && x.ProjectName == postedNote.ProjectName && x.Role >= UserRoles.ProjectManager);
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