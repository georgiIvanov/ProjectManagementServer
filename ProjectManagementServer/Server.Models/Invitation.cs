using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    public class Invitation
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId Id { get; set; }
        public string InvitedUser { get; set; }
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId InvitedUserId { get; set; }
        public string OrganizationName { get; set; }
    }
}
