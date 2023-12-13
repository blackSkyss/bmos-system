using AutoMapper;
using BMOS.BAL.DTOs;
using BMOS.BAL.DTOs.DashBoard;
using BMOS.BAL.DTOs.Meals;
using BMOS.BAL.DTOs.Products;
using BMOS.BAL.Exceptions;
using BMOS.BAL.Helpers;
using BMOS.BAL.Repositories.Interfaces;
using BMOS.DAL.Enums;
using BMOS.DAL.Infrastructures;
using BMOS.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Repositories.Implementations
{
    public class DashBoardRepository : IDashBoardRepository
    {
        private IMapper _mapper;
        private UnitOfWork _unitOfWork;
        public DashBoardRepository(IMapper mapper, IUnitOfWork unitOfWork)
        {
            this._mapper = mapper;
            this._unitOfWork = (UnitOfWork)unitOfWork;
        }

        #region GetStaffDashBoardAsync
        public async Task<GetStaffDashBoardResponse> GetStaffDashBoardAsync()
        {
            try
            {
                var meals = await _unitOfWork.DashBoardDAO.GetMealsAsync();
                var products = await _unitOfWork.DashBoardDAO.GetProductsAsync();
                var orders = await _unitOfWork.DashBoardDAO.GetOrdersAsync();
                var productInMeals = meals.SelectMany(m => m.ProductMeals.Select(pm => pm.Product)).Distinct().ToList();

                DateTime currentDate = DateTime.Now.Date;
                DateTime expiredDate = currentDate.AddDays(10).Date;

                // Get product sắp hết hạn trong vòng 10 ngày
                var expiringProducts = products.Where(product => product.ExpiredDate.Date >= currentDate
                                                              && product.ExpiredDate.Date <= expiredDate);

                //Get Product trong Meals sắp hết hạn trong vòng 10 ngày
                var productInMealsValid = productInMeals.Where(product => product.ExpiredDate.Date >= currentDate
                                                              && product.ExpiredDate.Date <= expiredDate);

                // Chuyển Products trong Meals thành Meals
                var expiringMeals = productInMealsValid.SelectMany(p => p.ProductMeals
                                                  .Select(pm => pm.Meal))
                                                  .Distinct().ToList();

                var boughtMeals = new List<GetMealsDashBoardResponse>();
                var doneOrders = orders.Where(x => x.OrderStatus == (int)OrderEnum.OrderStatus.DONE);
                foreach (var doneOrder in doneOrders)
                {
                    foreach (var orderDetail in doneOrder.OrderDetails)
                    {
                        if (boughtMeals.SingleOrDefault(x => x.ID == orderDetail.MealID) == null)
                        {
                            var boughtMeal = this._mapper.Map<GetMealsDashBoardResponse>(orderDetail.Meal);
                            boughtMeal.BoughtAmount += orderDetail.Quantity;
                            boughtMeals.Add(boughtMeal);
                        } else
                        {
                            boughtMeals.SingleOrDefault(x => x.ID == orderDetail.MealID).BoughtAmount += orderDetail.Quantity;
                        }
                    }
                }

                var boughtProducts = new List<GetProductDashBoardResponse>();
                foreach (var boughtMeal in boughtMeals)
                {
                    Meal meal = meals.SingleOrDefault(x => x.ID == boughtMeal.ID);
                    var boughtAmount = 0;
                    foreach (var productMeal in meal.ProductMeals)
                    {
                        if(boughtProducts.SingleOrDefault(x => x.ID == productMeal.ProductID) == null)
                        {
                            var boughtProduct = this._mapper.Map<GetProductDashBoardResponse>(productMeal.Product);
                            boughtProduct.BoughtAmount += productMeal.Amount;
                            boughtProducts.Add(boughtProduct);
                        } else
                        {
                            boughtProducts.SingleOrDefault(x => x.ID == productMeal.ProductID).BoughtAmount += productMeal.Amount;
                        }
                    }
                }

                Guid newID= new Guid();
                return new GetStaffDashBoardResponse
                {
                    Id = newID.ToString(),
                    TotalMeals = meals.Count(),
                    TotalProducts = products.Count(),
                    TotalNewOrders = orders.Where(x => x.OrderStatus == (int)OrderEnum.OrderStatus.NEWORDER).Count(),
                    TotalDoneOrders = orders.Where(x => x.OrderStatus == (int)OrderEnum.OrderStatus.DONE).Count(),
                    TotalMonthProfits = orders.Where(x => x.OrderStatus == (int)OrderEnum.OrderStatus.DONE
                                                       && x.OrderedDate.Month == DateTime.Today.Month).Sum(x => x.Total),
                    ExpirationMeals = _mapper.Map<List<GetMealsDashBoardResponse>>(expiringMeals),
                    ExpirationProducts = _mapper.Map<List<GetProductDashBoardResponse>>(expiringProducts),
                    BoughtMeals = boughtMeals,
                    BoughtProducts = boughtProducts
                };
            }
            catch (Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }
        }


        #endregion

        public async Task<GetStoreOwnerDashBoardResponse> GetStoreOwnerDashBoardAsync(int? year)
        {
            try
            {
                if(year == null)
                {
                    year = DateTime.Today.Year;
                }
                var customers = await this._unitOfWork.CustomerDAO.GetCustomersAsync();
                var staffs = await this._unitOfWork.StaffDAO.GetStaffsAsync();
                var meals = await _unitOfWork.DashBoardDAO.GetMealsAsync();
                var products = await _unitOfWork.DashBoardDAO.GetProductsAsync();
                List<Order> doneOrders = await this._unitOfWork.DashBoardDAO.GetDoneOrdersAsync();

                List<int?> profitYears = new List<int?>();
                List<GetMonthProfitResponse> MonthProfits = new List<GetMonthProfitResponse>();
                foreach (var doneOrder in doneOrders)
                {
                    if(profitYears.FirstOrDefault(x => x == doneOrder.OrderedDate.Year) == null)
                    {
                        profitYears.Add(doneOrder.OrderedDate.Year);
                    }
                    if(doneOrder.OrderedDate.Year == year)
                    {
                        GetMonthProfitResponse monthProfit = MonthProfits.SingleOrDefault(x => x.Month == doneOrder.OrderedDate.Month);
                        if (monthProfit  == null)
                        {
                            MonthProfits.Add(new GetMonthProfitResponse()
                            {
                                Month = doneOrder.OrderedDate.Month,
                                Profits = doneOrder.Total
                            });
                        } else
                        {
                            monthProfit.Profits += doneOrder.Total;
                        }
                    }
                }

                for (int i = 1; i <= 12; i++)
                {
                    GetMonthProfitResponse monthProfit = MonthProfits.SingleOrDefault(x => x.Month == i);
                    if(monthProfit == null)
                    {
                        MonthProfits.Add(new GetMonthProfitResponse()
                        {
                            Month = i,
                            Profits = 0
                        });
                    }
                }

                Guid newId = new Guid();
                return new GetStoreOwnerDashBoardResponse()
                {
                    Id = newId.ToString(),
                    TotalCustomers = customers.Count(),
                    TotalStaffs = staffs.Count(),
                    TotalMeals = meals.Count(),
                    TotalProducts = products.Count(),
                    ProfitYears = profitYears,
                    MonthProfits = MonthProfits
                };


            } catch(Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }
        }

        #region GetRevenueInMonthStaffAsync
        public async Task<GetRevenueResponse> GetRevenueInMonthStaffAsync(int month, int year)
        {
            try
            {
                if (month < 1 || month > 12)
                {
                    throw new BadRequestException("Invalid month. Month must be between 1 and 12.");
                }

                int currentYear = DateTime.Now.Year;
                if (year < 2010 || year > currentYear)
                {
                    throw new BadRequestException($"Invalid year. Year must be between 2010 and {currentYear}.");
                }

                var orders = await _unitOfWork.OrderDAO.GetOrdersAsync();
                orders = orders.Where(o => o.OrderedDate.Month == month &&
                                           o.OrderedDate.Year == year &&
                                           o.OrderStatus == (int)OrderEnum.OrderStatus.DONE).ToList();

                if (orders == null)
                {
                    throw new NotFoundException("No orders in this time");
                }

                //Get total Revenue in month
                var revenue = orders.Sum(o => o.Total);

                // get Meal in Month
                var mealInMonth = orders.SelectMany(o => o.OrderDetails
                                        .Select(odd => odd.Meal))
                                        .Distinct()
                                        .ToList();
                // get Product in Month
                var productInMonth = orders.SelectMany(o => o.OrderDetails
                                           .SelectMany(odd => odd.Meal.ProductMeals
                                           .Select(pro => pro.Product)))
                                           .Distinct()
                                           .ToList();
                GetRevenueResponse getRevenueResponse = new GetRevenueResponse
                {
                    Revenue = revenue,
                    MealsInMonth = _mapper.Map<List<GetMealsDashBoardResponse>>(mealInMonth),
                    ProductInMonth = _mapper.Map<List<GetProductDashBoardResponse>>(productInMonth)
                };
                return getRevenueResponse;
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
        #endregion

        #region GetDashBoardStoreOwnerAsync
        public async Task<GetStoreOwnerResponse> GetDashBoardStoreOwnerAsync(int month, int year)
        {
            try
            {
                if (month < 1 || month > 12)
                {
                    throw new BadRequestException("Invalid month. Month must be between 1 and 12.");
                }

                int currentYear = DateTime.Now.Year;
                if (year < 2010 || year > currentYear)
                {
                    throw new BadRequestException($"Invalid year. Year must be between 2010 and {currentYear}.");
                }
                var customers = await _unitOfWork.CustomerDAO.GetCustomersAsync();
                var products = await _unitOfWork.ProductDAO.GetProducts();
                var meals = await _unitOfWork.MealDAO.GetMealsAsync();
                var totalCustomer = customers.Where(c => c.Account.Role.ID == (int)RoleEnum.Role.CUSTOMER).Count();
                var totalStaff = customers.Where(c => c.Account.Role.ID == (int)RoleEnum.Role.STAFF).Count();

                var orders = await _unitOfWork.OrderDAO.GetOrdersAsync();
                orders = orders.Where(o => o.OrderedDate.Month == month &&
                                           o.OrderedDate.Year == year &&
                                           o.OrderStatus == (int)OrderEnum.OrderStatus.DONE).ToList();

                if (orders == null)
                {
                    throw new NotFoundException("No orders in this time");
                }

                // get Meal in Month
                var mealInMonth = orders.SelectMany(o => o.OrderDetails
                                        .Select(odd => odd.Meal))
                                        .Distinct()
                                        .ToList();
                // get Product in Month
                var productInMonth = orders.SelectMany(o => o.OrderDetails
                                           .SelectMany(odd => odd.Meal.ProductMeals
                                           .Select(pro => pro.Product)))
                                           .Distinct()
                                           .ToList();
                return new GetStoreOwnerResponse
                {
                    TotalCustomer = totalCustomer,
                    TotalStaff = totalStaff,
                    TotalMeal = meals.Count(),
                    TotalProduct = meals.Count(),
                    MealsInMonth = _mapper.Map<List<GetMealsDashBoardResponse>>(mealInMonth),
                    ProductInMonth = _mapper.Map<List<GetProductDashBoardResponse>>(productInMonth)
                };
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
        #endregion

        #region GetRevenueInYearStoreOwnerAsync
        public async Task<List<GetTotalInMonthResponse>> GetRevenueInYearStoreOwnerAsync(int? year)
        {
            try
            {
                int currentYear = DateTime.Now.Year;
                var orders = await _unitOfWork.OrderDAO.GetOrdersAsync();
                List<GetTotalInMonthResponse> monthlyTotals = new List<GetTotalInMonthResponse>();
                if (year != null)
                {
                    if (year < 2010 || year > currentYear)
                    {
                        throw new BadRequestException($"Invalid year. Year must be between 2010 and {currentYear}.");
                    }
                    // Get orders with year != null
                    var ordersWithYear = orders.Where(o => o.OrderedDate.Year == year &&
                                               o.OrderStatus == (int)OrderEnum.OrderStatus.DONE).ToList();

                    if (!ordersWithYear.Any())
                    {
                        throw new NotFoundException("No orders in this year");
                    }


                    // Calculate revenue in month of year
                    for (int month = 1; month <= 12; month++)
                    {
                        decimal monthlyTotal = orders
                            .Where(o => o.OrderedDate.Month == month)
                            .Sum(o => o.Total);

                        GetTotalInMonthResponse response = new GetTotalInMonthResponse
                        {
                            Month = month,
                            Total = monthlyTotal
                        };

                        monthlyTotals.Add(response);
                    }
                    return monthlyTotals;
                }
                else
                {
                    // Get orders with year == null
                    var ordersWithNullYear = orders.Where(o => o.OrderedDate.Year == currentYear &&
                                               o.OrderStatus == (int)OrderEnum.OrderStatus.DONE).ToList();
                    if (!ordersWithNullYear.Any())
                    {
                        throw new NotFoundException("No orders in this year");
                    }

                    // Calculate revenue in month of year
                    for (int month = 1; month <= 12; month++)
                    {
                        decimal monthlyTotal = orders
                            .Where(o => o.OrderedDate.Month == month)
                            .Sum(o => o.Total);

                        GetTotalInMonthResponse response = new GetTotalInMonthResponse
                        {
                            Month = month,
                            Total = monthlyTotal
                        };

                        monthlyTotals.Add(response);
                    }
                    return monthlyTotals;
                }
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

        #endregion
    }
}
