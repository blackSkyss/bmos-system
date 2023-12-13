using BMOS.BAL.DTOs.ProductImages;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.DTOs.Products
{
    public class PostProductRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime ExpiredDate { get; set; }
        public double Total { get; set; }
        public decimal Price { get; set; }
        public decimal OriginalPrice { get; set; }
        public List<IFormFile> ProductImages { get; set; }

    }
}
