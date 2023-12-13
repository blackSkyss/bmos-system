using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.DTOs.WalletTransactions.Momo
{
    public class QueryTransactionMomoRequest
    {
        public string? partnerCode { get; set; }
        public string? requestId { get; set; }
        public string? orderId { get; set; }
        public string? lang { get; set; }
        public string? signature { get; set; }
    }
}
