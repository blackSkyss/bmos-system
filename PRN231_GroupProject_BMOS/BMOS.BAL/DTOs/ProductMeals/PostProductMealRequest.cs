using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.DTOs.ProductMeals
{
    public class PostProductMealRequest
    { 
        public int ProductID { get; set; }
        public double Amount { get; set; }
    }
}
