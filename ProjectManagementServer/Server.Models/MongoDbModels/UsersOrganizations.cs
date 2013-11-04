using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models.MongoDbModels
{
    public class UsersOrganizations
    {
        public ObjectId Id { get; set; }
        public ObjectId UserId { get; set; }
        public ObjectId OrganizationId { get; set; }
        public string Name { get; set; }
    }
}
