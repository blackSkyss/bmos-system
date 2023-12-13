using AutoMapper;
using BMOS.BAL.DTOs.Products;
using BMOS.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Profiles.Products
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<PostProductRequest, Product>().ForMember(dest => dest.ProductImages, opt => opt.Ignore()).ReverseMap();
            CreateMap<UpdateProductRequest, Product>().ForMember(dest => dest.ProductImages, opt => opt.Ignore()).ReverseMap();
            CreateMap<Product, GetProductResponse>().ReverseMap();
            CreateMap<Product, GetProductDashBoardResponse>().ReverseMap();
        }
    }
}
