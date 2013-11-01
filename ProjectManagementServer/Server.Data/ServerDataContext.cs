using Server.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Data
{
    public class ServerDataContext : DbContext
    {
        public ServerDataContext()
            : base("AppHarborSQLServer")
        {

        }

        public virtual IDbSet<User> Users { get; set; }
        public virtual IDbSet<UserSecret> UsersSecrets { get; set; }
    }
}
