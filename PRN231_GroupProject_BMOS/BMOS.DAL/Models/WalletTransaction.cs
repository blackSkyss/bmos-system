using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.DAL.Models
{
    public class WalletTransaction
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string RechargeID { get; set; }
        public DateTime RechargeTime { get; set; }
        public decimal Amount { get; set; }
        public string Content { get; set; }
        public string TransactionType { get; set; }
        public int RechargeStatus { get; set; }
        public virtual Wallet Wallet { get; set; }
    }
}
