using BMOS.DAL.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.DAL.DAOs
{
    public class MealImageDAO
    {
        private BMOSDbContext _dbContext;
        public MealImageDAO(BMOSDbContext dbContext)
        {
            this._dbContext = dbContext;
        }
    }
}
