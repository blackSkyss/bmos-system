using BMOS.BAL.DTOs.WalletTransactions;
using BMOS.BAL.DTOs.WalletTransactions.Momo;
using BMOS.BAL.DTOs.WalletTransactions.Zalopay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Repositories.Interfaces
{
    public interface IWalletTransactionRepository
    {
        public Task<GetWalletTransactionResponse> CreateWalletTransactionAsync(PostWalletTransactionRequest wallet, MomoConfigModel config);
        public Task<PostTransactionZaloResponse> CreateZaloTransactionAsync(PostWalletTransactionRequest wallet, ZaloConfigModel config);
        public Task<GetWalletTransactionResponse> PaymentNotificationAsync(string Id, MomoConfigModel config);
        public Task ProcessWalletTransaction();
    }
}
