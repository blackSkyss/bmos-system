using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.DAL.Models
{
    public class OrderDetail
    {
        public int OrderID { get; set; }
        public int MealID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrices { get; set; }
        [ForeignKey("MealID")]
        public virtual Meal Meal { get; set; }
        [ForeignKey("OrderID")]
        public virtual Order Order { get; set; }

    }
}
