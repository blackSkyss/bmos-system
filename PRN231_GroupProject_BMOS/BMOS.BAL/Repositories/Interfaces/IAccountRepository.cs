using BMOS.BAL.DTOs.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Repositories.Interfaces
{
    public interface IAccountRepository
    {
        public Task<GetAccountResponse> ChangPassword(int accountId, ChangePasswordRequest changPasswordRequest);
    }
}
