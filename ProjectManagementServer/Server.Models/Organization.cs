using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    public class Organization
    {
        [JsonConverter(typeof(ObjectIdConverter))]
        public ObjectId Id { get; set; }
        [Required]
        //[BsonElement("n")]
        [StringLength(50, MinimumLength = 1)]
        public string Name { get; set; }
        //[BsonElement("desc")]
        [StringLength(300)]
        public string Description { get; set; }
        //[BsonElement("motto")]
        [StringLength(50)]
        public string Motto { get; set; }
        //[BsonElement("vi")]
        public string OrganizationVision { get; set; }
        //[BsonElement("usersInOrg")]
        public BsonDocument UsersIdsInOrganization { get; set; }
        //[BsonElement("projInOrg")]
        public BsonDocument ProjectsIdsInOrganization { get; set; }
        //[BsonElement("createdOn")]
        public DateTime DateCreated { get; set; }
    }
}
