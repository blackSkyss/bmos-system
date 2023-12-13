using AutoMapper;
using BMOS.BAL.DTOs.Tokens;
using BMOS.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Profiles.Tokens
{
    public class TokenProfile : Profile
    {
        public TokenProfile()
        {
            CreateMap<Token, GetTokenResponse>().ReverseMap();
        }
    }
}
