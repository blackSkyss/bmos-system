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
    public class OrderTransactionDAO
    {
        private BMOSDbContext _dbContext;
        public OrderTransactionDAO(BMOSDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        #region Get order transactions
        public async Task<IEnumerable<OrderLog>> GetOrderTransactionsAsync()
        {
            try
            {
                return await _dbContext.OrderLogs
                                       .Include(ot => ot.Order)
                                       .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Get order transactions by orderId
        public async Task<IEnumerable<OrderLog>> GetOrderTransactionsByOrderIdAsync(int id)
        {
            try
            {
                return await _dbContext.OrderLogs.Where(ot => ot.Order.ID == id)
                                                         .Include(ot => ot.Order)
                                                         .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion
    }
}
