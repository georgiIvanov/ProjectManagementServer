using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    public class User
    {
        [NotMapped]
        [BsonId]
        public ObjectId _MongoId { get; set; }
        public virtual int Id { get; set; }
        [Required]
        [StringLength(20, MinimumLength=3)]
        public virtual string Username { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public virtual string Email { get; set; }
        [StringLength(40, MinimumLength=36)]
        public virtual string AuthKey { get; set; }
        [DataType(DataType.Date)]
        public virtual DateTime LastLogin { get; set; }

        [StringLength(25)]
        public string MongoId { get; set; }
    }
}
