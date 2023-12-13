using AutoMapper;
using BMOS.BAL.DTOs.ProductMeals;
using BMOS.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Profiles.ProductMeals
{
    public class ProductMealProfile: Profile
    {
        public ProductMealProfile()
        {
            CreateMap<ProductMeal, GetProductByProductMealsResponse>().ReverseMap();
            CreateMap<ProductMeal, GetProductMealResponse>().ReverseMap();
            CreateMap<ProductMeal, PostProductMealRequest>().ReverseMap();
        }
    }
}
