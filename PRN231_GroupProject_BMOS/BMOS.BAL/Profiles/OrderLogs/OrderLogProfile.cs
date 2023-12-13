using AutoMapper;
using BMOS.BAL.DTOs.OrderTransactions;
using BMOS.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Profiles.OrderTransactions
{
    public class OrderLogProfile: Profile
    {
        public OrderLogProfile()
        {
            CreateMap<OrderLog, GetOrderLogResponse>().ReverseMap();
        }
    }
}
