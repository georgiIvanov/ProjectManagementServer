using Server.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Data
{
    public class ServerData : IUoWData
    {
        private readonly DbContext context;
        private readonly Dictionary<Type, object> repositories = new Dictionary<Type, object>();

        public ServerData()
            :this(new ServerDataContext())
        {
        }

        public ServerData(ServerDataContext context)
        {
            this.context = context;
        }

        public DbContext Context
        {
            get { return this.context; }
        }

        public int SaveChanges()
        {
            return this.context.SaveChanges();
        }

        public void Dispose()
        {
            if (this.context != null)
            {
                this.context.Dispose();
            }
        }

        private IRepository<T> GetRepository<T>() where T : class
        {
            if (!this.repositories.ContainsKey(typeof(T)))
            {
                var type = typeof(GenericRepository<T>);

                this.repositories.Add(typeof(T), Activator.CreateInstance(type, this.context));
            }

            return (IRepository<T>)this.repositories[typeof(T)];
        }

        public IRepository<User> Users
        {
            get
            {
                return this.GetRepository<User>();
            }
            set { }
        }

        public IRepository<UserSecret> UserSecrets
        {
            get
            {
                return this.GetRepository<UserSecret>();
            }
            set { }
        }

    }
}
