using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMOS.DAL.Models;

namespace BMOS.BAL.DTOs.Wallets
{
    public class GetWalletsResponse
    {
        [Key]
        public int AccountID { get; set; }
        public decimal Balance { get; set; }
        public virtual Account Account { get; set; }
        public virtual ICollection<WalletTransaction> WalletTransactions { get; set; }
    }
}
