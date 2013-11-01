﻿using Server.Data;
using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ServerApp.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }

        [HttpGet]
        public IEnumerable<User> Haha()
        {
            ServerDataContext db = new ServerDataContext();
            var users = db.Users.ToList();

            var js = Json(users);
            return users;
        }

        [HttpPost]
        public User PostUser(User user)
        {
            return user;
        }

        [HttpGet]
        public User GetUser()
        {
            User u = new User()
            {
                Id = 5,
                AuthKey = "djwadwajd1",
                Email = "fafa@gmail.com",
                LastLogin = DateTime.Now
            };

            return u;
        }
    }
}
