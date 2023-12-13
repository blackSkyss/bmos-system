using BMOS.BAL.DTOs.ProductImages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.DTOs.Products
{
    public class GetProductDashBoardResponse
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime ImportedTime { get; set; }
        public DateTime ExpiredDate { get; set; }
        public decimal Price { get; set; }
        public double Total { get; set; }
        public int Status { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int ModifiedBy { get; set; }
        public double BoughtAmount { get; set; }
        public ICollection<GetProductImageResponse> ProductImages { get; set; }
    }
}
