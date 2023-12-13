using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.DTOs.WalletTransactions.Zalopay
{
    public class PostTransactionZaloRequest
    {
        public int? app_id { get; set; }
        public string? app_user { get; set; }
        public string? app_time { get; set; }
        public decimal? amount { get; set; }
        public string? app_trans_id { get; set; }
        public string? bank_code { get; set; }
        public string? embed_data { get; set; }
        public string? item { get; set; }
        public string? callback_url { get; set; }
        public string? description { get; set; }
        public string? mac { get; set; }
    }
}
