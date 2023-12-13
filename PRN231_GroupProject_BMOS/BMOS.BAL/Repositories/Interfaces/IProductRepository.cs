using BMOS.BAL.DTOs.FireBase;
using BMOS.BAL.DTOs.Products;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMOS.BAL.DTOs.Products;

namespace BMOS.BAL.Repositories.Interfaces
{
    public interface IProductRepository
    {
        /*public Task<GetProductResponse> GetProductByID(int id);*/
        public Task<List<GetProductResponse>> GetProductsAsync();
        public Task<GetProductResponse> CreateNewProductAsync(PostProductRequest postProductRequest, FireBaseImage fireBaseImage, HttpContext httpContext);
        public Task<GetProductResponse> UpdateProductAsync(
                                                           int productId,
                                                           UpdateProductRequest updateProductRequest,
                                                           FireBaseImage fireBaseImage,
                                                           HttpContext httpContext);
        public Task DeleteProductAsync(int id, HttpContext httpContext);
        public Task<GetProductResponse> GetProductDetailByIdAsync(int id);
        public Task<List<GetProductResponse>> GetActiveProducts();
        public Task<GetProductResponse> GetActiveProduct(int productId);
    }
}
