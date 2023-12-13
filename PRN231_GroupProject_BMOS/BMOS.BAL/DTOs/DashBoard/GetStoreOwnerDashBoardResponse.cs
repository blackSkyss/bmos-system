using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.DTOs.DashBoard
{
    public class GetStoreOwnerDashBoardResponse
    {
        [Key]
        [JsonIgnore]
        public string Id { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalStaffs { get; set; }
        public int TotalProducts { get; set; }
        public int TotalMeals { get; set; }
        public List<int?> ProfitYears { get; set; }
        public List<GetMonthProfitResponse> MonthProfits { get; set; }
    }
}
