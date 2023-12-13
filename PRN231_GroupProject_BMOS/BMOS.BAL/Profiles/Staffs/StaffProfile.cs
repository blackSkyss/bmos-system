using AutoMapper;
using BMOS.BAL.DTOs.Staffs;
using BMOS.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Profiles.Staffs
{
    public class StaffProfile: Profile
    {
        public StaffProfile()
        {
            CreateMap<Staff, GetStaffResponse>().ReverseMap();
            CreateMap<Staff, PostStaffRequest>().ReverseMap();
        }
    }
}
