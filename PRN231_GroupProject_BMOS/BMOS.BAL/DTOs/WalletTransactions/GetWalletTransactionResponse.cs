using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.DTOs.WalletTransactions
{
    public class GetWalletTransactionResponse
    {
        [Key]
        public string RechargeID { get; set; }
        public DateTime RechargeTime { get; set; }
        public decimal Amount { get; set; }
        public string? Content { get; set; }
        public string? TransactionType { get; set; }
        public int RechargeStatus { get; set; }
        public string? PayUrl { get; set; }
        public string? Deeplink { get; set; }
        public string? QrCodeUrl { get; set; }
        public string? Applink { get; set; }

    }
}
