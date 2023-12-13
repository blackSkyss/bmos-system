using AutoMapper;
using BMOS.BAL.DTOs.Customers;
using BMOS.BAL.DTOs.FireBase;
using BMOS.BAL.DTOs.Meals;
using BMOS.BAL.DTOs.ProductMeals;
using BMOS.BAL.DTOs.Products;
using BMOS.BAL.Exceptions;
using BMOS.BAL.Helpers;
using BMOS.BAL.Repositories.Interfaces;
using BMOS.DAL.DAOs;
using BMOS.DAL.Enums;
using BMOS.DAL.Infrastructures;
using BMOS.DAL.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Repositories.Implementations
{
    public class MealRepository : IMealRepository
    {
        private UnitOfWork _unitOfWork;
        private IMapper _mapper;
        public MealRepository(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = (UnitOfWork)unitOfWork;
            _mapper = mapper;
        }

        public async Task<GetMealResponse> GetMealByID(int id)
        {
            try
            {
                Meal meal = await this._unitOfWork.MealDAO.GetMealByID(id);
                if (meal == null)
                {
                    throw new NotFoundException("Meal not found");
                }
                GetMealResponse mealDTO = this._mapper.Map<GetMealResponse>(meal);
                Staff staff = await this._unitOfWork.StaffDAO.GetStaffDetailAsync(meal.ModifiedBy);
                mealDTO.ModifiedStaff = staff.FullName;
                return mealDTO;
            }
            catch (NotFoundException ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new NotFoundException(error);
            }
            catch (Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }
        }

        public async Task<List<GetMealResponse>> GetMealsAsync()
        {
            try
            {
                List<Meal> meals = await this._unitOfWork.MealDAO.GetMealsAsync();
                List<GetMealResponse> mealsDTO = new List<GetMealResponse>(); 
                foreach (var meal in meals)
                {
                    Staff staff = await this._unitOfWork.StaffDAO.GetStaffDetailAsync(meal.ModifiedBy);
                    meal.Price = meal.ProductMeals.Sum(x => x.Price);
                    GetMealResponse mealDTO = this._mapper.Map<GetMealResponse>(meal);
                    mealDTO.ModifiedStaff = staff.FullName;
                    mealsDTO.Add(mealDTO);
                }
                return mealsDTO;
            }
            catch (Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }
        }

        public async Task<List<GetMealResponse>> GetAvailableMeals()
        {
            try
            {
                List<Meal> meals = await this._unitOfWork.MealDAO.GetAvailableMeals();
                List<GetMealResponse> mealsDTO = new List<GetMealResponse>();
                foreach (var meal in meals)
                {
                    Staff staff = await this._unitOfWork.StaffDAO.GetStaffDetailAsync(meal.ModifiedBy);
                    meal.Price = meal.ProductMeals.Sum(x => x.Price);
                    GetMealResponse mealDTO = this._mapper.Map<GetMealResponse>(meal);
                    mealDTO.ModifiedStaff = staff.FullName;
                    mealsDTO.Add(mealDTO);
                }
                return mealsDTO;
            }
            catch (Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }
        }


        //create meal
        public async Task<GetMealResponse> CreateMeal(FireBaseImage fireBaseImage, PostMealRequest newMeal, HttpContext httpContext)
        {
            try
            {
                JwtSecurityToken jwtSecurityToken = TokenHelper.ReadToken(httpContext);
                string emailFromClaim = jwtSecurityToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Email).Value;
                var accountStaff = await _unitOfWork.StaffDAO.GetStaffByEmailAsync(emailFromClaim);
                
                Meal meal = this._mapper.Map<Meal>(newMeal);
                meal.MealImages = new List<MealImage>();
                string productsError = "";
                string productAmountErrors = "";
                foreach (var productMeal in newMeal.ProductMeals)
                {
                    Product existedProduct = await this._unitOfWork.ProductDAO.GetProductByIdAsync(productMeal.ProductID);
                    if (existedProduct == null)
                    {
                        productsError += $"[ProductId - {productMeal.ProductID}] does not exist in the system.\n";
                    }
                    if (productMeal.Amount > existedProduct.Total)
                    {
                        productAmountErrors += $"[ProductId - {productMeal.ProductID}] does not have enough amount to provide for this meal.\n";
                    }
                    var existedProductMeal = meal.ProductMeals.SingleOrDefault(x => x.ProductID == productMeal.ProductID);
                    existedProductMeal.Price = existedProduct.Price * (decimal)productMeal.Amount;
                }
                meal.Price = meal.ProductMeals.Sum(x => x.Price);
                if (productsError.Length > 0)
                {
                    throw new NotFoundException(productsError);
                }
                if (productAmountErrors.Length > 0)
                {
                    throw new BadRequestException(productAmountErrors);
                }

                foreach (var image in newMeal.MealFileImages)
                {
                    if (image != null)
                    {
                        FileHelper.SetCredentials(fireBaseImage);
                        FileStream fileStream = FileHelper.ConvertFormFileToStream(image);
                        Tuple<string, string> result = await FileHelper.UploadImage(fileStream, "Meal");
                        meal.MealImages.Add(new MealImage
                        {
                            Source = result.Item1,
                            ImageID = result.Item2,
                        });
                    }
                }
                meal.Status = (int)MealEnum.Status.STOCKING;
                meal.ModifiedDate = DateTime.Now;
                meal.ModifiedBy = accountStaff.AccountID;
                await this._unitOfWork.MealDAO.AddMeal(meal);
                await this._unitOfWork.CommitAsync();
                GetMealResponse mealDTO = this._mapper.Map<GetMealResponse>(meal);
                mealDTO.ModifiedStaff = accountStaff.FullName;
                return mealDTO;
            }
            catch (NotFoundException ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new NotFoundException(error);
            }
            catch (BadRequestException ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new BadRequestException(error);
            }
            catch (Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }
        }

        //Update meal
        public async Task<GetMealResponse> UpdateMeal(int ID, FireBaseImage fireBaseImage, UpdateMealRequest updateMealRequest, HttpContext httpContext)
        {
            try
            {
                JwtSecurityToken jwtSecurityToken = TokenHelper.ReadToken(httpContext);
                string emailFromClaim = jwtSecurityToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Email).Value;
                var accountStaff = await _unitOfWork.StaffDAO.GetStaffByEmailAsync(emailFromClaim);

                Meal meal = await this._unitOfWork.MealDAO.GetMealByID(ID);
                meal.ProductMeals = new List<ProductMeal>();
                if (meal == null)
                {
                    throw new NotFoundException("Meal does not exist in the system.");
                }
                string productsError = "";
                string productAmountErrors = "";
                string productStatusErrors = "";
                foreach (var productMeal in updateMealRequest.ProductMeals)
                {
                    Product existedProduct = await this._unitOfWork.ProductDAO.GetProductByIdAsync(productMeal.ProductID);
                    if( existedProduct == null)
                    {
                        productsError += $"[ProductId - {productMeal.ProductID}] does not exist in the system.\n";
                    } else
                    {
                        if(existedProduct.Total < productMeal.Amount)
                        {
                            productAmountErrors += $"[ProductId - {productMeal.ProductID}] - {existedProduct.Name} - does not have enough amount to provide for this meal.\n";
                        }
                        if(existedProduct.Status == (int)ProductEnum.Status.INACTIVE)
                        {
                            productStatusErrors += $"[ProductId - {productMeal.ProductID}] - {existedProduct.Name} - is inactive action status, so this meal can not update.\n";
                        }
                        ProductMeal existedProductMeal = meal.ProductMeals.SingleOrDefault(x => x.ProductID == productMeal.ProductID);
                        //productmeal exist in meal
                        if(existedProductMeal != null)
                        {
                            existedProductMeal.Amount = productMeal.Amount;
                        }
                        //productmeal not exist in meal
                        if(existedProductMeal == null)
                        {
                            meal.ProductMeals.Add(new ProductMeal()
                            {
                                ProductID = productMeal.ProductID,
                                Amount = productMeal.Amount,
                                Price = existedProduct.Price * (decimal)productMeal.Amount,
                            });
                        }
                    }
                }
                meal.Price = meal.ProductMeals.Sum(x => x.Price);
                if (productsError.Length > 0)
                {
                    throw new NotFoundException(productsError);
                }
                if(productAmountErrors.Length > 0)
                {
                    throw new BadRequestException(productAmountErrors);
                }
                if(productStatusErrors.Length > 0)
                {
                    throw new BadRequestException(productStatusErrors);
                }
                foreach (var oldProductMeal in meal.ProductMeals)
                {
                    if(updateMealRequest.ProductMeals.SingleOrDefault(x => x.ProductID == oldProductMeal.ProductID) == null)
                    {
                        meal.ProductMeals.Remove(oldProductMeal);
                    }
                }

                FileHelper.SetCredentials(fireBaseImage);
                if (updateMealRequest.RemoveMealImages != null && updateMealRequest.RemoveMealImages.Count > 0)
                {
                    foreach (var removeMealImages in updateMealRequest.RemoveMealImages)
                    {
                        MealImage removedImage = meal.MealImages.SingleOrDefault(x => x.Source.ToLower().Equals(removeMealImages.ToLower()));
                        if (removedImage != null)
                        {
                            await FileHelper.DeleteImageAsync(removedImage.ImageID, "Meal");
                            meal.MealImages.Remove(removedImage);
                        }
                        else
                        {
                            throw new BadRequestException($"Remove Image URL: {removeMealImages} does not exist in this Meal.");
                        }
                    }
                }
                if (updateMealRequest.NewMealImages != null && updateMealRequest.NewMealImages.Count > 0)
                {
                    foreach (var newProductImage in updateMealRequest.NewMealImages)
                    {
                        FileStream fileStream = FileHelper.ConvertFormFileToStream(newProductImage);
                        Tuple<string, string> result = await FileHelper.UploadImage(fileStream, "Product");
                        var mealImage = new MealImage
                        {
                            Source = result.Item1,
                            ImageID = result.Item2
                        };
                        meal.MealImages.Add(mealImage);
                    }
                }

                meal.Title = updateMealRequest.Title;
                meal.Description = updateMealRequest.Description;
                meal.Status = updateMealRequest.Status;
                meal.ModifiedDate = DateTime.Now;
                meal.ModifiedBy = accountStaff.AccountID;

                this._unitOfWork.MealDAO.UpdateMeal(meal);
                await this._unitOfWork.CommitAsync();
                GetMealResponse mealDTO = this._mapper.Map<GetMealResponse>(meal);
                mealDTO.ModifiedStaff = accountStaff.FullName;
                return mealDTO;
            }
            catch (NotFoundException ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new NotFoundException(error);
            }
            catch(BadRequestException ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new BadRequestException(error);
            }
            catch (Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }
        }

        //Delete meal by update status
        public async Task DeleteMeal(int ID, HttpContext httpContext)
        {
            try
            {
                JwtSecurityToken jwtSecurityToken = TokenHelper.ReadToken(httpContext);
                string emailFromClaim = jwtSecurityToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Email).Value;
                var accountStaff = await _unitOfWork.AccountDAO.GetAccountByEmail(emailFromClaim);

                Meal meal = await this._unitOfWork.MealDAO.GetMealByID(ID);
                if (meal == null)
                {
                    throw new NotFoundException("Meal does not exist in the system.");
                }
                meal.Status = (int)MealEnum.Status.INACTIVE;
                meal.ModifiedDate = DateTime.Now;
                meal.ModifiedBy = accountStaff.ID;

                this._unitOfWork.MealDAO.UpdateMeal(meal);
                await this._unitOfWork.CommitAsync();
            }
            catch (NotFoundException ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new NotFoundException(error);
            }
            catch (Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }
        }

        public async Task<GetMealResponse> GetAvailableMeal(int mealId)
        {
            try
            {
                Meal meal = await this._unitOfWork.MealDAO.GetAvailableMeal(mealId);
                if(meal == null)
                {
                    throw new NotFoundException("Meal does not exist in the system.");
                }
                return this._mapper.Map<GetMealResponse>(meal);
            }
            catch (NotFoundException ex)
            {
                string error = ErrorHelper.GetErrorString("Meal ID", ex.Message);
                throw new NotFoundException(error);
            }
            catch (Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }
        }
    }
}
