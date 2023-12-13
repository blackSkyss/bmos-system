using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.DAL.Models
{
    public class Wallet
    {
        [Key]
        public int AccountID { get; set; }
        public decimal Balance { get; set; }
        public virtual Account Account { get; set; }
        public virtual ICollection<WalletTransaction> WalletTransactions { get; set; }
    }
}
