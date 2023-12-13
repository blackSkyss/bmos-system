using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.DTOs.WalletTransactions
{
    public class PostWalletTransactionRequest
    {
        public string Email { get; set; }
        public decimal Amount { get; set; }
        public string RedirectUrl { get; set; }
    }
}
