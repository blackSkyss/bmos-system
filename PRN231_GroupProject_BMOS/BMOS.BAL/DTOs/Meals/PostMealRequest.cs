using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMOS.BAL.DTOs.MealImages;
using BMOS.BAL.DTOs.ProductMeals;
using BMOS.DAL.Models;
using Microsoft.AspNetCore.Http;

namespace BMOS.BAL.DTOs.Meals
{
    public class PostMealRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public virtual ICollection<IFormFile> MealFileImages { get; set; }
        public virtual ICollection<PostProductMealRequest> ProductMeals { get; set; }
    }
}
