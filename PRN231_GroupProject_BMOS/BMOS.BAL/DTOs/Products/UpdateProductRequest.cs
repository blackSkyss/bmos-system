using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.DTOs.Products
{
    public class UpdateProductRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime ExpiredDate { get; set; }
        public double Total { get; set; }
        public decimal Price { get; set; }
        public decimal OriginalPrice { get; set; }
        public int Status { get; set; }
        public List<IFormFile>? NewProductImages { get; set; }
        public List<string>? RemoveProductImages { get; set; }
    }
}
