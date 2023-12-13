using AutoMapper;
using BMOS.BAL.DTOs.Orders;
using BMOS.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Profiles.Orders
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, GetOrderResponse>().ReverseMap();
        }
    }
}
