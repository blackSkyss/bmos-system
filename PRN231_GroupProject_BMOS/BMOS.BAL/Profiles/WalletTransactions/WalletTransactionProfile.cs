using AutoMapper;
using BMOS.BAL.DTOs.WalletTransactions;
using BMOS.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Profiles.WalletTransactions
{
    public class WalletTransactionProfile: Profile
    {
        public WalletTransactionProfile()
        {
            CreateMap<WalletTransaction, GetWalletTransactionResponse>().ReverseMap();
        }
    }
}
