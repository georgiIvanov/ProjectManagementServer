using Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Data
{
    public interface IUoWData : IDisposable
    {
        IRepository<User> Users { get; set; }
        IRepository<UserSecret> UserSecrets { get; set; }
        int SaveChanges();
    }
}
