using AutoMapper;
using BMOS.BAL.DTOs.Roles;
using BMOS.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Profiles.Roles
{
    public class RoleProfile: Profile
    {
        public RoleProfile()
        {
            CreateMap<Role, GetRoleResponse>().ReverseMap();
        }
    }
}
