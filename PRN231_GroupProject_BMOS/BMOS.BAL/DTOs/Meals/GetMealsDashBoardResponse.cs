using BMOS.BAL.DTOs.MealImages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.DTOs.Meals
{
    public class GetMealsDashBoardResponse
    {
        [Key]
        public int ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Status { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int ModifiedBy { get; set; }
        public double BoughtAmount { get; set; }
        public ICollection<GetMealImageResponse> MealImages { get; set; }
    }
}
