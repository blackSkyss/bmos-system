using BMOS.BAL.DTOs.FireBase;
using BMOS.BAL.DTOs.Staffs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Repositories.Interfaces
{
    public interface IStaffRepository
    {
        public Task<IEnumerable<GetStaffResponse>> GetStaffsAsync();
        public Task<GetStaffResponse> GetStaffDetailAsync(int id);
        public Task<GetStaffResponse> CreateStaffAsync(PostStaffRequest request, FireBaseImage fireBaseImage);
        public Task<GetStaffResponse> UpdateStaffAsync(int staffId, UpdateStaffRequest request, FireBaseImage fireBaseImage);
        public Task DeteleStaffAsync(int id);
    }
}
