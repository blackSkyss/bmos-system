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
    public class StaffDAO
    {
        private BMOSDbContext _dbContext;
        public StaffDAO(BMOSDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        #region Get staffs
        public async Task<IEnumerable<Staff>> GetStaffsAsync()
        {

            try
            {
                return await this._dbContext.Staffs
                                            .Include(s => s.Account).ThenInclude(x => x.Role)
                                            .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Get detail staff
        public async Task<Staff> GetStaffDetailAsync(int id)
        {

            try
            {
                return await this._dbContext.Staffs
                                            .Include(s => s.Account).ThenInclude(acc => acc.Role)
                                            .SingleOrDefaultAsync(s => s.AccountID == id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Get staff by email
        public async Task<Staff> GetStaffByEmailAsync(string email)
        {
            try
            {
                return await _dbContext.Staffs.Include(c => c.Account)
                                                 .SingleOrDefaultAsync(c => c.Account.Email.Equals(email));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Get staff by phone
        public async Task<Staff> GetStaffByPhoneAsync(string phone)
        {
            try
            {
                return await _dbContext.Staffs.Include(c => c.Account)
                                                 .SingleOrDefaultAsync(c => c.Phone.Equals(phone));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Get staff by identity
        public async Task<Staff> GetStaffByIdentityAsync(string identity)
        {
            try
            {
                return await _dbContext.Staffs.Include(c => c.Account)
                                                 .SingleOrDefaultAsync(c => c.IdentityNumber.Equals(identity));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Create staff
        public async Task CreateStaffAsync(Staff staff)
        {
            try
            {
                await this._dbContext.Staffs.AddAsync(staff);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Update staff
        public void UpdateStaff(Staff staff)
        {
            try
            {
                this._dbContext.Entry<Staff>(staff).State = EntityState.Modified;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion
    }
}
