using AutoMapper;
using BMOS.BAL.DTOs.Accounts;
using BMOS.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Profiles.Accounts
{
    public class AccountProfile : Profile
    {
        public AccountProfile()
        {
            CreateMap<Account, GetAccountResponse>().ForMember(dept => dept.RoleName, opt => opt.MapFrom(src => src.Role.Name)).ReverseMap();
            CreateMap<Account, PostAccountRequest>().ReverseMap();
        }
    }
}
