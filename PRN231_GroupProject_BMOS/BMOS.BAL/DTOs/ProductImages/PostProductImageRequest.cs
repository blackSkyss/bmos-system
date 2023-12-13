using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.DTOs.ProductImages
{
    public class PostProductImageRequest
    {
        public IFormFile Source { get; set; }
    }
}
