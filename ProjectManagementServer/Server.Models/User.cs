using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    public class User
    {
        public virtual int Id { get; set; }
        [Required]
        [StringLength(20, MinimumLength=3)]
        public virtual string Username { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public virtual string Email { get; set; }
        [StringLength(38
        public virtual string AuthKey { get; set; }
        [DataType(DataType.Date)]
        public virtual DateTime LastLogin { get; set; }
    }
}
