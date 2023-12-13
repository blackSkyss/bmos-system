using AutoMapper;
using BMOS.BAL.DTOs.FireBase;
using BMOS.BAL.DTOs.Products;
using BMOS.BAL.Errors;
using BMOS.BAL.Exceptions;
using BMOS.BAL.Helpers;
using BMOS.BAL.Repositories.Interfaces;
using BMOS.DAL.Enums;
using BMOS.DAL.Infrastructures;
using BMOS.DAL.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Repositories.Implementations
{
    public class ProductRepository : IProductRepository
    {
        private UnitOfWork _unitOfWork;
        private IMapper _mapper;
        public ProductRepository(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = (UnitOfWork)unitOfWork;
            _mapper = mapper;
        }

        #region GetProducts
        public async Task<List<GetProductResponse>> GetProductsAsync()
        {
            try
            {
                List<Product> products = await this._unitOfWork.ProductDAO.GetProducts();
                return this._mapper.Map<List<GetProductResponse>>(products);
            }
            catch (Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }
        }
        #endregion

        #region CreateNewProductAsync
        public async Task<GetProductResponse> CreateNewProductAsync(PostProductRequest postProductRequest, FireBaseImage fireBaseImage, HttpContext httpContext)
        {
            try
            {
                var images = new List<ProductImage>();
                JwtSecurityToken jwtSecurityToken = TokenHelper.ReadToken(httpContext);
                string emailFromClaim = jwtSecurityToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Email).Value;
                var accountStaff = await _unitOfWork.AccountDAO.GetAccountByEmail(emailFromClaim);

                Product product = this._mapper.Map<Product>(postProductRequest);

                product.ModifiedBy = accountStaff.ID;
                product.ImportedTime = DateTime.Now;
                product.ModifiedDate = DateTime.Now;
                product.Status = (int)ProductEnum.Status.STOCKING;

                #region Upload image to firebase

                foreach (var image in postProductRequest.ProductImages)
                {
                    FileStream fileStream = FileHelper.ConvertFormFileToStream(image);
                    FileHelper.SetCredentials(fireBaseImage);
                    Tuple<string, string> result = await FileHelper.UploadImage(fileStream, "Product");
                    var productImage = new ProductImage
                    {
                        Source = result.Item1,
                        ImageID = result.Item2
                    };
                    images.Add(productImage);
                }
                product.ProductImages = images;

                #endregion
                await _unitOfWork.ProductDAO.CreateNewProduct(product);
                await this._unitOfWork.CommitAsync();
                return this._mapper.Map<GetProductResponse>(product);
            }
            catch (Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }

        }
        #endregion

        #region UpdateProductAsync
        public async Task<GetProductResponse> UpdateProductAsync(int productId, UpdateProductRequest updateProductRequest, FireBaseImage fireBaseImage, HttpContext httpContext)
        {
            try
            {
                Product product = await _unitOfWork.ProductDAO.GetProductByIdAsync(productId);
                if (product == null)
                {
                    throw new NotFoundException("product Id does not exist in the system.");
                }
                if (updateProductRequest.Status != (int)ProductEnum.Status.INACTIVE && updateProductRequest.Status != (int)ProductEnum.Status.STOCKING)
                {
                    throw new BadRequestException("Status must be 1 or 0.");
                }
                var images = new List<ProductImage>();
                JwtSecurityToken jwtSecurityToken = TokenHelper.ReadToken(httpContext);
                string emailFromClaim = jwtSecurityToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Email).Value;
                var accountStaff = await _unitOfWork.AccountDAO.GetAccountByEmail(emailFromClaim);

                product = _mapper.Map(updateProductRequest, product);

                //Inactive
                if (updateProductRequest.Status == (int)ProductEnum.Status.INACTIVE)
                {
                    foreach (var productMeal in product.ProductMeals)
                    {
                        productMeal.Meal.Status = (int)MealEnum.Status.INACTIVE;
                    }
                }
                //active
                if (updateProductRequest.Status == (int)ProductEnum.Status.STOCKING)
                {
                    double totalAmount = 0d;
                    product.ProductMeals = product.ProductMeals.OrderBy(x => x.Amount).ToList();
                    foreach (var productMeal in product.ProductMeals)
                    {
                        totalAmount += productMeal.Amount;
                        if (totalAmount > updateProductRequest.Total)
                        {
                            productMeal.Meal.Status = (int)MealEnum.Status.INACTIVE;
                        }
                        else
                        {
                            productMeal.Meal.Status = (int)MealEnum.Status.STOCKING;
                        }

                    }
                }

                product.ModifiedBy = accountStaff.ID;
                product.ModifiedDate = DateTime.Now;

                #region Upload image to firebase
                FileHelper.SetCredentials(fireBaseImage);
                if (updateProductRequest.RemoveProductImages != null && updateProductRequest.RemoveProductImages.Count > 0)
                {
                    foreach (var removeProductImage in updateProductRequest.RemoveProductImages)
                    {
                        ProductImage removedImage = product.ProductImages.SingleOrDefault(x => x.Source.ToLower().Equals(removeProductImage.ToLower()));
                        if (removedImage != null)
                        {
                            await FileHelper.DeleteImageAsync(removedImage.ImageID, "Product");
                            product.ProductImages.Remove(removedImage);
                        }
                        else
                        {
                            throw new BadRequestException($"Remove Image URL: {removeProductImage} does not exist in this product.");
                        }
                    }
                }

                if (updateProductRequest.NewProductImages != null && updateProductRequest.NewProductImages.Count > 0)
                {
                    foreach (var newProductImage in updateProductRequest.NewProductImages)
                    {
                        FileStream fileStream = FileHelper.ConvertFormFileToStream(newProductImage);
                        Tuple<string, string> result = await FileHelper.UploadImage(fileStream, "Product");
                        var productImage = new ProductImage
                        {
                            Source = result.Item1,
                            ImageID = result.Item2
                        };
                        product.ProductImages.Add(productImage);
                    }
                }
                #endregion

                _unitOfWork.ProductDAO.UpdateProduct(product);
                await this._unitOfWork.CommitAsync();
                return this._mapper.Map<GetProductResponse>(product);
            }
            catch (NotFoundException ex)
            {
                string error = ErrorHelper.GetErrorString("Product Id", ex.Message);
                throw new NotFoundException(error);
            }
            catch (BadRequestException ex)
            {
                string fieldNameError = "";
                if (ex.Message.ToLower().Contains("status"))
                {
                    fieldNameError = "Status";
                }
                else if (ex.Message.ToLower().Contains("remove image url"))
                {
                    fieldNameError = "RemoveProductImages";
                }
                string error = ErrorHelper.GetErrorString(fieldNameError, ex.Message);
                throw new BadRequestException(error);
            }
            catch (Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }
        }
        #endregion

        #region DeleteProductAsync
        public async Task DeleteProductAsync(int id, HttpContext httpContext)
        {
            try
            {
                JwtSecurityToken jwtSecurityToken = TokenHelper.ReadToken(httpContext);
                string emailFromClaim = jwtSecurityToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Email).Value;
                var accountStaff = await _unitOfWork.AccountDAO.GetAccountByEmail(emailFromClaim);
                var product = await _unitOfWork.ProductDAO.GetProductByIdAsync(id);
                if (product == null)
                {
                    throw new NotFoundException("Product id does not exist in the system.");
                }
                product.Status = (int)ProductEnum.Status.INACTIVE;
                if (product.ProductMeals != null && product.ProductMeals.Count() > 0)
                {
                    foreach (var productMeal in product.ProductMeals)
                    {
                        productMeal.Meal.Status = (int)MealEnum.Status.INACTIVE;
                    }
                }
                product.ModifiedDate = DateTime.Today;
                product.ModifiedBy = accountStaff.ID;
                _unitOfWork.ProductDAO.UpdateProduct(product);
                await _unitOfWork.CommitAsync();

            }
            catch (NotFoundException ex)
            {
                string error = ErrorHelper.GetErrorString("Product Id", ex.Message);
                throw new NotFoundException(error);
            }
            catch (Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }
        }
        #endregion

        #region GetProductDetailByIdAsync
        public async Task<GetProductResponse> GetProductDetailByIdAsync(int id)
        {
            try
            {
                var product = await _unitOfWork.ProductDAO.GetProductByIdAsync(id);
                if (product == null)
                {
                    throw new NotFoundException("Product id does not exist in the system.");
                }
                var productDTO = _mapper.Map<GetProductResponse>(product);
                Staff staff = await this._unitOfWork.StaffDAO.GetStaffDetailAsync(product.ModifiedBy);
                productDTO.ModifiedStaff = staff.FullName;
                return productDTO;
            }
            catch (NotFoundException ex)
            {
                string error = ErrorHelper.GetErrorString("Product Id", ex.Message);
                throw new NotFoundException(error);
            }
            catch (Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }
        }

        public async Task<List<GetProductResponse>> GetActiveProducts()
        {
            try
            {
                List<Product> products = await this._unitOfWork.ProductDAO.GetActiveProducts();
                return this._mapper.Map<List<GetProductResponse>>(products);
            }
            catch (Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }
        }

        public async Task<GetProductResponse> GetActiveProduct(int productId)
        {
            try
            {
                Product product = await this._unitOfWork.ProductDAO.GetActiveProduct(productId);
                if (product == null)
                {
                    throw new NotFoundException("Product does not exist in the system.");
                }
                return this._mapper.Map<GetProductResponse>(product);
            }
            catch (NotFoundException ex)
            {
                string error = ErrorHelper.GetErrorString("Product Id", ex.Message);
                throw new NotFoundException(error);
            }
            catch (Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }
        }
        #endregion
    }
}
