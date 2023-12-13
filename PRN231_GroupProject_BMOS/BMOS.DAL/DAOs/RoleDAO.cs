using BMOS.DAL.DBContext;
using BMOS.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.DAL.DAOs
{
    public class RoleDAO
    {
        private BMOSDbContext _dbContext;
        public RoleDAO(BMOSDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<Role> GetRoleAsync(int roleId)
        {
            try
            {
                return await this._dbContext.Roles.SingleOrDefaultAsync(x => x.ID == roleId);
            } catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #region Get role by id
        public async Task<Role> GetRoleDetailAsync(int id)
        {

            try
            {
                return await this._dbContext.Roles.SingleOrDefaultAsync(r => r.ID == id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion
    }
}
