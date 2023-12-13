using BMOS.BAL.DTOs.Meals;
using BMOS.BAL.DTOs.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.DTOs.DashBoard
{
    public class GetStoreOwnerResponse
    {
        public int TotalCustomer { get; set; }
        public int TotalStaff { get; set; }
        public int TotalProduct { get; set; }
        public int TotalMeal { get; set; }
        public List<GetMealsDashBoardResponse> MealsInMonth { get; set; }
        public List<GetProductDashBoardResponse> ProductInMonth { get; set; }
    }
}

