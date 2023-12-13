using AutoMapper;
using BMOS.BAL.DTOs.Products;
using BMOS.BAL.DTOs.Wallets;
using BMOS.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Profiles.Wallets
{
    public class WalletProfile: Profile
    {
        public WalletProfile()
        {
            CreateMap<Wallet, GetWalletsResponse>().ReverseMap();
        }
    }
}
