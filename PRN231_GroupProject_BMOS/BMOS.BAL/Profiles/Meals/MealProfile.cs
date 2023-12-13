using AutoMapper;
using BMOS.BAL.DTOs.Meals;
using BMOS.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Profiles.Meals
{
    public class MealProfile : Profile
    {
        public MealProfile()
        {
            CreateMap<Meal, GetMealResponse>().ReverseMap();
            CreateMap<Meal, GetMealFromProduct>().ReverseMap();
            CreateMap<Meal, GetMealsDashBoardResponse>().ReverseMap();
            CreateMap<Meal, PostMealRequest>().ReverseMap();
            CreateMap<Meal, UpdateMealRequest>().ReverseMap();
        }
    }
}
