using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.DAL.Models
{
    public class ProductMeal
    {
        public int ProductID { get; set; }
        public int MealID { get; set; }
        public double Amount { get; set; }
        public decimal Price { get; set; }

        [ForeignKey("ProductID")]
        public virtual Product Product { get; set; }
        [ForeignKey("MealID")]
        public virtual Meal Meal { get; set; }
    }
}
