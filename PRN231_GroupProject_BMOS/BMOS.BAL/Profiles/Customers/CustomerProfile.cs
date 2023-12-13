using AutoMapper;
using BMOS.BAL.DTOs.Customers;
using BMOS.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Profiles.Customers
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            CreateMap<Customer, GetCustomerResponse>().ReverseMap();
            CreateMap<Customer, UpdateCustomerRequest>().ReverseMap();
        }
    }
}
