using BMOS.DAL.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.DAL.Infrastructures
{
    public interface IDbFactory: IDisposable
    {
        public BMOSDbContext InitDbContext();
    }
}
