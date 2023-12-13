using AutoMapper;
using BMOS.BAL.DTOs.ProductImages;
using BMOS.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Profiles.ProductImages
{
    public class ProductImageProfile: Profile
    {
        public ProductImageProfile()
        {
            CreateMap<ProductImage, GetProductImageResponse>().ReverseMap();
        }
    }
}
