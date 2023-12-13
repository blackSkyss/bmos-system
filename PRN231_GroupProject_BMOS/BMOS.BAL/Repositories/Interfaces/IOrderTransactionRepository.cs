using BMOS.BAL.DTOs.OrderTransactions;
using BMOS.DAL.DAOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Repositories.Interfaces
{
    public interface IOrderTransactionRepository
    {
        public Task<IEnumerable<GetOrderLogResponse>> GetOrderTransactionsAsync();
        public Task<IEnumerable<GetOrderLogResponse>> GetOrderTransactionsByOrderIdAsync(int? id);
    }
}
