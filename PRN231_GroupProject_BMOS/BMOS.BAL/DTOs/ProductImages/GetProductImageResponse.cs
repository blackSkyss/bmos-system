using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.DTOs.ProductImages
{
    public class GetProductImageResponse
    {
        [Key]
        public string Source { get; set; }
    }
}
