using Server.Data;
using Server.Models;
using ServerApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Net;
using System.Web.Http;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using ServerApp.Utilities;

namespace ServerApp.Controllers
{
    public class AccountController : ApiController
    {
        IUoWData db;

        public AccountController(IUoWData db)
        {
            this.db = db;
        }

        [HttpPost]
        public HttpResponseMessage Register(RegisteredUser regUser)
        {
            HttpResponseMessage responseMessage;

            if (!ModelState.IsValid)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid credentials.");
                return responseMessage;
            }

            if (db.Users.All().FirstOrDefault(x => x.Email == regUser.Email) != null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "User with this email already exists.");
                return responseMessage;
            }

            if (db.Users.All().FirstOrDefault(x => x.Username == regUser.Username) != null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Username already exists.");
                return responseMessage;
            }

            User newUser = new User();

            newUser.Username = regUser.Username;
            newUser.Email = regUser.Email;
            newUser.LastLogin = DateTime.Now;
            newUser.AuthKey = Guid.NewGuid().ToString();

            UserSecret newUserSecret = new UserSecret()
            {
                User = newUser,
                Usersecret = regUser.PasswordSecret
            };

            MongoDatabase mongoDb = MongoClientFactory.GetDatabase();
            var usersCollection = mongoDb.GetCollection(MongoCollections.Users);
            usersCollection.Save(newUser);
            //var findFromMongo = usersCollection.FindOne(Query.EQ("Username", newUser.Username));
            //newUser.MongoId = findFromMongo["_id"].AsString;

            newUser.MongoId = newUser._MongoId.ToString();

            db.UserSecrets.Add(newUserSecret);
            db.Users.Add(newUser);
            db.SaveChanges();

            LoginResult logResult = new LoginResult()
            {
                AuthKey = newUser.AuthKey,
                LastLogged = newUser.LastLogin.ToString("yyyy-MM-dd HH:mm:ss")
            };

            responseMessage = this.Request.CreateResponse(HttpStatusCode.OK,
                logResult);

            return responseMessage;
        }

        [HttpPut]
        public HttpResponseMessage Login(LoginUser logUser)
        {
            HttpResponseMessage responseMessage;// = new HttpResponseMessage();

            if (!ModelState.IsValid)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid credentials.");
                return responseMessage;
            }

            var found = db.Users.All().FirstOrDefault(
                x => x.Username == logUser.Username || x.Email == logUser.Email);

            if (found == null)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid username or password.");
                return responseMessage;
            }

            var secret = db.UserSecrets.All().FirstOrDefault(
                x => x.Usersecret == logUser.PasswordSecret);

            if (secret == null || secret.Usersecret != logUser.PasswordSecret)
            {
                responseMessage = this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid username or password.");
                return responseMessage;
            }

            found.AuthKey = Guid.NewGuid().ToString();
            LoginResult logResult = new LoginResult()
            {
                AuthKey = found.AuthKey,
                LastLogged = found.LastLogin.ToString("yyyy-MM-dd HH:mm:ss")
            };

            found.LastLogin = DateTime.Now;

            db.Users.Update(found);
            db.SaveChanges();

            responseMessage = this.Request.CreateResponse(HttpStatusCode.OK, 
                logResult);
            
            return responseMessage;
        }
	}
}