using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMOS.BAL.DTOs.Wallets;
using BMOS.DAL.Models;

namespace BMOS.BAL.Repositories.Interfaces
{
    public interface IWalletRepository
    {
        public Task<GetWalletsResponse> GetWalletByUserID(int userID);
    }
}
