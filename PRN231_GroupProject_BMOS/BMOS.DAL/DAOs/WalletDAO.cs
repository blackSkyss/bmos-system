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
    public class WalletDAO
    {
        private BMOSDbContext _dbContext;
        public WalletDAO(BMOSDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        //get wallet by userID
        public async Task<Wallet> GetWalletByUserID(int userID)
        {
            try
            {
                return await _dbContext.Wallets.Include(x => x.Account).Include(x => x.WalletTransactions)
                                               .SingleOrDefaultAsync(w => w.AccountID == userID 
                                               && w.Account.Status == Convert.ToBoolean(AccountEnum.Status.ACTIVE));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        #region Get wallet by accountId
        public async Task<Wallet> GetWalletByAccountIdAsync(int accountId)
        {
            try
            {
                return await this._dbContext.Wallets.Where(w => w.AccountID == accountId)
                                                          .Include(w => w.Account)
                                                          .SingleOrDefaultAsync();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Update wallet
        public void UpdateWallet(Wallet wallet)
        {
            try
            {
                this._dbContext.Entry<Wallet>(wallet).State = EntityState.Modified;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        public async Task CreateWalletAsync(Wallet wallet)
        {
            try
            {
                await this._dbContext.Wallets.AddAsync(wallet);
            } catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
