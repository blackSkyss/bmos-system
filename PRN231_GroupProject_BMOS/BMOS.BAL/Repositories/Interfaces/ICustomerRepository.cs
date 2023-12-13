using BMOS.BAL.DTOs.Customers;
using BMOS.BAL.DTOs.FireBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        public Task<GetCustomerResponse> GetCustomerByAccountIdAsync(int accountId);
        public Task<GetCustomerResponse> UpdateCustomerProfileByAccountIdAsync(int accountId,
                                                                               FireBaseImage fireBaseImage,
                                                                               UpdateCustomerRequest updateCustomerRequest);
        public Task<List<GetCustomerResponse>> GetCustomersAsync();

        public Task<GetCustomerResponse> BanCustomerAsync(int accountId);

        public Task<GetCustomerResponse> Register (FireBaseImage fireBaseImage, RegisterRequest registerRequest);
    }
}
