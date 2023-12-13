using AutoMapper;
using BMOS.BAL.DTOs.MealImages;
using BMOS.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Profiles.MealImages
{
    public class MealImageProfile: Profile
    {
        public MealImageProfile()
        {
            CreateMap<MealImage, GetMealImageResponse>().ReverseMap();
        }
    }
}
