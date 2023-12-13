using BMOS.BAL.DTOs.Meals;
using BMOS.BAL.DTOs.Products;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.DTOs
{
    public class GetStaffDashBoardResponse
    {
        [Key]
        [JsonIgnore]
        public string Id { get; set; }
        public int TotalProducts { get; set; }
        public int TotalMeals { get; set; }
        public int TotalDoneOrders { get; set; }
        public int TotalNewOrders { get; set; }
        public decimal TotalMonthProfits { get; set; }
        public List<GetProductDashBoardResponse> ExpirationProducts { get; set; }
        public List<GetMealsDashBoardResponse> ExpirationMeals { get; set; }
        public List<GetMealsDashBoardResponse> BoughtMeals { get; set; }
        public List<GetProductDashBoardResponse> BoughtProducts { get; set; }
    }
}
