using AutoMapper;
using BMOS.BAL.DTOs.OrderDetails;
using BMOS.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Profiles.OrderDetails
{
    public class OrderDetailProfile: Profile
    {
        public OrderDetailProfile()
        {
            CreateMap<OrderDetail, GetOrderDetailResponse>().ReverseMap();
        }
    }
}
