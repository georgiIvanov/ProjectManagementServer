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
            : base("LocalDbConnection")
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserSecret> UsersSecrets { get; set; }

    }
}
