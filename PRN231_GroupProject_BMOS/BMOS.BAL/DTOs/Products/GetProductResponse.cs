using BMOS.BAL.DTOs.ProductImages;
using BMOS.BAL.DTOs.ProductMeals;
using BMOS.DAL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.DTOs.Products
{
    public class GetProductResponse
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime ImportedTime { get; set; }
        public DateTime ExpiredDate { get; set; }
        public decimal Price { get; set; }
        public decimal OriginalPrice { get; set; }
        public double Total { get; set; }
        public int Status { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string? ModifiedStaff { get; set; }
        public ICollection<GetProductImageResponse> ProductImages { get; set; }
        public ICollection<GetProductMealResponse> ProductMeals { get; set; }
    }
}
