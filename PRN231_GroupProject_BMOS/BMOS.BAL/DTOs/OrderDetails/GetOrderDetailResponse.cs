using BMOS.BAL.DTOs.Meals;
using BMOS.DAL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.DTOs.OrderDetails
{
    public class GetOrderDetailResponse
    {
        [Key]
        public int OrderID { get; set; }
        public int MealID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrices { get; set; }
        public virtual GetMealFromProduct Meal { get; set; }
    }
}
