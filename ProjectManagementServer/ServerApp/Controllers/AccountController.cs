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
        public string Register(RegisteredUser regUser)
        {
            if (!ModelState.IsValid)
            {
                return "Invalid credentials";
            }

            if (db.Users.All().FirstOrDefault(x => x.Email == regUser.Email) != null)
            {
                return "User with this email already exists";
            }

            if (db.Users.All().FirstOrDefault(x => x.Username == regUser.Username) != null)
            {
                return "Username already exists";
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

            db.UserSecrets.Add(newUserSecret);
            db.Users.Add(newUser);
            db.SaveChanges();

            return newUser.AuthKey;
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
            found.LastLogin = DateTime.Now;

            db.Users.Update(found);
            db.SaveChanges();

            responseMessage = this.Request.CreateResponse(HttpStatusCode.OK, 
                found.AuthKey);
            
            return responseMessage;
        }
	}
}