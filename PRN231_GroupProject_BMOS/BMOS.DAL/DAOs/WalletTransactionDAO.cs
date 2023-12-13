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
    public class WalletTransactionDAO
    {
        private BMOSDbContext _dbContext;
        public WalletTransactionDAO(BMOSDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        #region Get all wallet transaction
        public async Task<IEnumerable<WalletTransaction>> GetWalletTransactionsAsync()
        {
            try
            {
                return await _dbContext.WalletTransactions.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Get all wallet transaction(Status: pending)
        public async Task<IEnumerable<WalletTransaction>> GetWalletTransactionsPendingAsync()
        {
            try
            {
                return await _dbContext.WalletTransactions.Where(w => w.RechargeStatus == 0).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Get wallet transaction by ID
        public async Task<WalletTransaction> GetWalletTransactionByRechargeIDAsync(string Id)
        {
            try
            {
                return await _dbContext.WalletTransactions.Where(wt => wt.RechargeID == Id)
                                                           .Include(wt => wt.Wallet)
                                                           .ThenInclude(w => w.Account).SingleOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Create wallet transaction
        public async Task CreateWalletTransactionAsync(WalletTransaction wallet)
        {
            try
            {
                await this._dbContext.WalletTransactions.AddAsync(wallet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Update wallet transaction
        public void UpdateWalletTransaction(WalletTransaction walletTransaction)
        {
            try
            {
                this._dbContext.Entry<WalletTransaction>(walletTransaction).State = EntityState.Modified;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

    }
}
