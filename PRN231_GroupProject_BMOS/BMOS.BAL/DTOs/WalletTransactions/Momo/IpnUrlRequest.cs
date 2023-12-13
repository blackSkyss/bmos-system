using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.DTOs.WalletTransactions.Momo
{
    public class IpnUrlRequest
    {
        public string? partnerCode { get; set; }
        public string? orderId { get; set; }
        public string? requestId { get; set; }
        public decimal? amount { get; set; }
        public string? orderInfo { get; set; }
        public string? partnerUserId { get; set; }
        public string? orderType { get; set; }
        public string? transId { get; set; }
        public int? resultCode { get; set; }
        public string? message { get; set; }
        public string? payType { get; set; }
        public long? responseTime { get; set; }
        public string? extraData { get; set; }
        public string? signature { get; set; }
    }
}
