using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMOS.BAL.DTOs.Meals;
using BMOS.BAL.DTOs.Products;
using BMOS.DAL.Models;

namespace BMOS.BAL.DTOs.ProductMeals
{
    public class GetProductByProductMealsResponse
    {
        [Key]
        public int MealID { get; set; }
        public double Amount { get; set; }
        public decimal Price { get; set; }
        public GetProductResponse Product { get; set; }
    }
}
