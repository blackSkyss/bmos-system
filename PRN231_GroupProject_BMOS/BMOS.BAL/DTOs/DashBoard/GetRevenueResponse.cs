using BMOS.BAL.DTOs.Meals;
using BMOS.BAL.DTOs.Products;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.DTOs.DashBoard
{
    public class GetRevenueResponse
    {
        [Key]
        public decimal Revenue { get; set; }
        public List<GetMealsDashBoardResponse> MealsInMonth { get; set; }
        public List<GetProductDashBoardResponse> ProductInMonth { get; set; }
    }
}
