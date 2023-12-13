using BMOS.DAL.DBContext;
using BMOS.DAL.Enums;
using BMOS.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.DAL.DAOs
{
    public class AccountDAO
    {
        private BMOSDbContext _dbContext;
        public AccountDAO(BMOSDbContext dbContext)
        {
            this._dbContext = dbContext;
        }


        #region Get account by email and password
        public async Task<Account> GetAccountByEmailAndPasswordAsync(string email, string password)
        {
            try
            {
                return await _dbContext.Accounts.Include(a => a.Role)
                                       .SingleOrDefaultAsync(a => a.Email.Equals(email.Trim().ToLower()) 
                                                               && a.PasswordHash.Equals(password) 
                                                               && a.Status == Convert.ToBoolean(AccountEnum.Status.ACTIVE));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
        #endregion

        public async Task<Account> GetAccountById(int accountId)
        {
            try
            {
                return await _dbContext.Accounts.Include(a => a.Role)
                                           .Include(a => a.Tokens)
                                           .SingleOrDefaultAsync(a => a.ID == accountId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public void ChangePassword(Account account)
        {
            try
            {
                this._dbContext.Entry<Account>(account).State = EntityState.Modified;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Account> GetAccountByEmail(string email)
        {

            try
            {
                return await this._dbContext.Accounts.SingleOrDefaultAsync(a => a.Email.Equals(email) && a.Status == Convert.ToBoolean(AccountEnum.Status.ACTIVE));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task AddNewAccount(Account account)
        {
            try
            {
                await _dbContext.AddAsync(account);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
