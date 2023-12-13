using BMOS.DAL.DBContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.DAL.Infrastructures
{
    public class DbFactory : Disposable, IDbFactory
    {
        private BMOSDbContext _dbContext;

        public DbFactory()
        {

        }

        public BMOSDbContext InitDbContext()
        {
            if (_dbContext == null)
            {
                _dbContext = new BMOSDbContext();
            }
            return _dbContext;
        }

        protected override void DisposeCore()
        {
            if (this._dbContext != null)
            {
                this._dbContext.Dispose();
            }
        }
    }
}
