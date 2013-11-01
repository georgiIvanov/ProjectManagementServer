using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    public class UserSecret
    {
        public virtual int Id { get; set; }
        [StringLength(40, MinimumLength=40)]
        public virtual string Usersecret { get; set; }
        public virtual User User { get; set; }

    }
}
