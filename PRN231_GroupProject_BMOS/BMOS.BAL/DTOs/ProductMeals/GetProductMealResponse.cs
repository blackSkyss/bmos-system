using BMOS.BAL.DTOs.Meals;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.DTOs.ProductMeals
{
    public class GetProductMealResponse
    {
        [Key]
        public int ProductID { get; set; }
        public int MealID { get; set; }
        public double Amount { get; set; }
        public decimal Price { get; set; }
        public GetMealFromProduct Meal { get; set; }
    }
}
