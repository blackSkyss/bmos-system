using BMOS.BAL.DTOs;
using BMOS.BAL.DTOs.DashBoard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Repositories.Interfaces
{
    public interface IDashBoardRepository
    {
        public Task<GetStaffDashBoardResponse> GetStaffDashBoardAsync();
        public Task<GetStoreOwnerDashBoardResponse> GetStoreOwnerDashBoardAsync(int? year);
        public Task<GetRevenueResponse> GetRevenueInMonthStaffAsync(int month, int year);
        public Task<GetStoreOwnerResponse> GetDashBoardStoreOwnerAsync(int month, int year);
        public Task<List<GetTotalInMonthResponse>> GetRevenueInYearStoreOwnerAsync(int? year);
    }
}
