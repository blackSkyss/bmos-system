using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.DAL.Models
{
    public class Product
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal OriginalPrice { get; set; }
        public DateTime ImportedTime { get; set; }
        public DateTime ExpiredDate { get; set; }
        public double Total { get; set; }
        public int Status { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int ModifiedBy { get; set; }
        public virtual ICollection<ProductMeal> ProductMeals { get; set; }
        public virtual ICollection<ProductImage> ProductImages { get; set; }
    }
}
