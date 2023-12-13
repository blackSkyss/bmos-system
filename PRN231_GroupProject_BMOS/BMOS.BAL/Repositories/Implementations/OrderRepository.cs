using AutoMapper;
using BMOS.BAL.DTOs.Orders;
using BMOS.BAL.Exceptions;
using BMOS.BAL.Helpers;
using BMOS.BAL.Repositories.Interfaces;
using BMOS.DAL.Enums;
using BMOS.DAL.Infrastructures;
using BMOS.DAL.Models;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Quartz.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Repositories.Implementations
{
    public class OrderRepository : IOrderRepository
    {
        private UnitOfWork _unitOfWork;
        private IMapper _mapper;
        public OrderRepository(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = (UnitOfWork)unitOfWork;
            _mapper = mapper;
        }

        #region Checkout
        public async Task<GetOrderResponse> CheckOutAsync(PostOrderRequest request)
        {
            try
            {
                // Seeding OwnerStore have account with id = 1
                var walletOwnerStore = await _unitOfWork.WalletDAO.GetWalletByAccountIdAsync((int)RoleEnum.Role.OWNERSTORE);

                #region Validation
                var customer = await _unitOfWork.CustomerDAO.GetCustomerByEmailAsync(request.Email);
                if (customer == null)
                {
                    throw new NotFoundException("Customer does not exist.");
                }
                var walletCustomer = await _unitOfWork.WalletDAO.GetWalletByAccountIdAsync(customer.AccountID);

                var totalOrder = 0m;
                var orderDetails = new List<OrderDetail>();
                //tổng số product của 1 order
                List<Product> productsOfOrder = new List<Product>();
                List<Meal> meals = new List<Meal>();
                foreach (var meal in request.Meals)
                {

                    var mealExist = await _unitOfWork.MealDAO.GetMealByIdAsync(meal.Id);
                    if (mealExist == null)
                    {
                        // Add exception for each instance
                        throw new NotFoundException($"[Meal-{meal.Id}] does not exist.");
                    }

                    if (mealExist.Status == (int)MealEnum.Status.OUTOFSTOCK)
                    {
                        throw new BadRequestException($"[Meal-{meal.Id}] is out of stock.");
                    }
                    else if (mealExist.Status == (int)MealEnum.Status.INACTIVE)
                    {
                        throw new BadRequestException($"[Meal-{meal.Id}] is inactive.");
                    }
                    else
                    {
                        meals.Add(mealExist);
                        foreach (var productMeal in mealExist.ProductMeals)
                        {
                            var productOfOrder = productsOfOrder.SingleOrDefault(x => x.ID == productMeal.ProductID);
                            if (productOfOrder == null)
                            {
                                // nếu product đó chưa có trong list productsOfOrder -> add product đó vào
                                productsOfOrder.Add(new Product()
                                {
                                    ID = productMeal.ProductID,
                                    Total = productMeal.Amount * meal.Amount
                                });
                            }
                            else
                            {
                                // tăng số lượng của product đó dựa trên amount của meal sử dụng
                                productOfOrder.Total += productMeal.Amount * meal.Amount;
                            }
                        }
                    }
                }
                List<string> mealErrors = new List<string>();
                foreach (var product in productsOfOrder)
                {
                    var existedProduct = await this._unitOfWork.ProductDAO.GetProductByIdAsync(product.ID);
                    if (product.Total > existedProduct.Total)
                    {
                        //lấy danh sách meal liên quan đến product này
                        var existedProductMeals = existedProduct.ProductMeals;
                        foreach (var productMeal in existedProductMeals)
                        {
                            Meal mealError = meals.SingleOrDefault(m => m.ID == productMeal.MealID);
                            if (mealError != null)
                            {
                                string mealErrorStr = $"[Meal - {mealError.Title}] contains [Product - {existedProduct.Name}] that is not enough quality to create this meal.";
                                mealErrors.Add(mealErrorStr);
                            }
                        }
                    }
                }
                if (mealErrors.Count > 0)
                {
                    string error = "";
                    foreach (var mealError in mealErrors)
                    {
                        error += mealError + "\n";
                    }
                    throw new BadRequestException(error);
                }
                foreach (var meal in request.Meals)
                {
                    var totalPriceOfMeal = 0m;
                    var existedMeal = meals.SingleOrDefault(x => x.ID == meal.Id);
                    foreach (var productMeal in existedMeal.ProductMeals)
                    {
                        totalPriceOfMeal += (productMeal.Price * meal.Amount);
                    }
                    orderDetails.Add(new OrderDetail
                    {
                        MealID = meal.Id,
                        Quantity = meal.Amount,
                        UnitPrices = totalPriceOfMeal
                    });

                    totalOrder += totalPriceOfMeal;
                }
                #endregion

                #region Save order and wallet transaction
                // Minus balance wallet of user and Plus wallet for Owner store (if success)
                if (walletCustomer.Balance < totalOrder)
                {
                    throw new BadRequestException("Your account balance is not enough for payment.");
                }
                else
                {
                    #region Create Order and order transaction
                    // Create order, order transaction
                    var orderTransactions = new List<OrderLog>();
                    OrderLog orderTransaction = new OrderLog
                    {
                        PaymentTime = DateTime.Now,
                    };

                    Order order = new Order
                    {
                        OrderedDate = DateTime.Now,
                        Total = totalOrder,
                        Customer = customer,
                        OrderDetails = orderDetails,
                    };
                    #endregion

                    #region Save order, order detail and order transaction
                    order.OrderStatus = (int)OrderEnum.OrderStatus.NEWORDER;
                    orderTransaction.Status = (int)OrderLogEnum.Status.PAID;
                    orderTransactions.Add(orderTransaction);
                    order.OrderTransactions = orderTransactions;

                    await _unitOfWork.OrderDAO.CreateOrderAsync(order);
                    #endregion

                    #region Save wallet and wallet transaction
                    // Minus balance user wallet (transactionType = SEND)
                    WalletTransaction walletTransactionCustomer = new WalletTransaction
                    {
                        RechargeID = DateTime.Now.Ticks.ToString(),
                        RechargeTime = DateTime.Now,
                        Amount = totalOrder,
                        Content = "Payment order of BMOS",
                        TransactionType = WalletTransactionEnum.TransactionType.SEND.ToString(),
                        RechargeStatus = (int)WalletTransactionEnum.RechangeStatus.SUCCESSED,
                        Wallet = walletCustomer

                    };

                    await _unitOfWork.WalletTransactionDAO.CreateWalletTransactionAsync(walletTransactionCustomer);

                    walletCustomer.Balance -= totalOrder;
                    _unitOfWork.WalletDAO.UpdateWallet(walletCustomer);

                    // Plus balance owner wallet (transactionType = 2(RECEIVE))
                    WalletTransaction walletTransactionOwner = new WalletTransaction
                    {
                        RechargeID = DateTime.Now.Ticks.ToString(),
                        RechargeTime = DateTime.Now,
                        Amount = totalOrder,
                        Content = $"Receive payment order of [{customer.Account.Email} - {customer.FullName}]",
                        TransactionType = WalletTransactionEnum.TransactionType.RECEIVE.ToString(),
                        RechargeStatus = (int)WalletTransactionEnum.RechangeStatus.SUCCESSED,
                        Wallet = walletOwnerStore

                    };

                    await _unitOfWork.WalletTransactionDAO.CreateWalletTransactionAsync(walletTransactionOwner);

                    walletOwnerStore.Balance += totalOrder;
                    _unitOfWork.WalletDAO.UpdateWallet(walletOwnerStore);
                    #endregion

                    #region Minus Amount of product
                    // Amount of product -= (Amount of order meal * Amount of product meal)
                    // Change status of product and meal if total = 0
                    foreach (var orderProduct in productsOfOrder)
                    {
                        Product product = await _unitOfWork.ProductDAO.GetProductByIdAsync(orderProduct.ID);
                        product.Total -= orderProduct.Total;
                        if (product.Total <= 0)
                        {
                            product.Total = 0;
                            product.Status = (int)ProductEnum.Status.OUTOFSTOCK;
                            _unitOfWork.ProductDAO.UpdateProduct(product);

                            var productMeals = await _unitOfWork.ProductMealDAO.GetProductMealByProductIdAsync(product.ID);
                            foreach (var productMeal in productMeals)
                            {
                                var meal = await _unitOfWork.MealDAO.GetMealByIdAsync(productMeal.MealID);
                                if (meal.Status == (int)MealEnum.Status.STOCKING)
                                {
                                    meal.Status = (int)MealEnum.Status.OUTOFSTOCK;
                                    _unitOfWork.MealDAO.UpdateMeal(meal);
                                }
                            }
                        }
                    }
                    #endregion

                    await _unitOfWork.CommitAsync();

                    return _mapper.Map<GetOrderResponse>(order);
                }
                #endregion
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

        #region Processing order
        public async Task ConfirmOrderToProcessingAsync(int orderId)
        {
            try
            {
                #region Validation
                var order = await _unitOfWork.OrderDAO.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    throw new NotFoundException($"[Order - {orderId}] does not exist.");
                }

                if (order.OrderStatus == (int)OrderEnum.OrderStatus.PROCESSING)
                {
                    throw new BadRequestException($"[Order - {orderId}] is already in the processing.");
                }

                if (order.OrderStatus == (int)OrderEnum.OrderStatus.DONE)
                {
                    throw new BadRequestException($"[Order - {orderId}] has been processed successfully before.");
                }

                if (order.OrderStatus == (int)OrderEnum.OrderStatus.CANCELLED)
                {
                    throw new BadRequestException($"[Order - {orderId}] has been canceled.");
                }
                #endregion

                #region Update order, create order log
                var orderLog = new OrderLog
                {
                    PaymentTime = DateTime.Now,
                    Status = (int)OrderLogEnum.Status.PROCESSING,
                };

                order.OrderStatus = (int)OrderEnum.OrderStatus.PROCESSING;
                order.OrderTransactions.Add(orderLog);

                _unitOfWork.OrderDAO.UpdateOrder(order);
                await _unitOfWork.CommitAsync();
                #endregion
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

        #region Done order
        public async Task ConfirmOrderToDoneAsync(int orderId)
        {
            try
            {
                #region Validation
                var order = await _unitOfWork.OrderDAO.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    throw new NotFoundException($"[Order - {orderId}] does not exist.");
                }

                if (order.OrderStatus == (int)OrderEnum.OrderStatus.NEWORDER)
                {
                    throw new BadRequestException($"[Order - {orderId}] hasn't been processed yet.");
                }

                if (order.OrderStatus == (int)OrderEnum.OrderStatus.DONE)
                {
                    throw new BadRequestException($"[Order - {orderId}] is already done.");
                }

                if (order.OrderStatus == (int)OrderEnum.OrderStatus.CANCELLED)
                {
                    throw new BadRequestException($"[Order - {orderId}] has been canceled.");
                }
                #endregion

                #region Update order, create order log
                var orderLog = new OrderLog
                {
                    PaymentTime = DateTime.Now,
                    Status = (int)OrderLogEnum.Status.DONE,
                };

                order.OrderStatus = (int)OrderEnum.OrderStatus.DONE;
                order.OrderTransactions.Add(orderLog);

                _unitOfWork.OrderDAO.UpdateOrder(order);
                await _unitOfWork.CommitAsync();
                #endregion
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

        #region Cancel order
        public async Task CancelOrderAsync(int customerId, int orderId)
        {
            try
            {
                #region Validation
                var customer = await _unitOfWork.CustomerDAO.GetCustomerByIdAsync(customerId);
                if (customer == null)
                {
                    throw new NotFoundException("Customer does not exist.");
                }

                var order = customer.Orders.SingleOrDefault(x => x.ID == orderId);
                if (order == null)
                {
                    throw new NotFoundException("Customer does not have this order.");
                }

                if (order.OrderStatus == (int)OrderEnum.OrderStatus.PROCESSING)
                {
                    throw new BadRequestException("Order cannot cancel when the meal preparation is processing.");
                }

                if (order.OrderStatus == (int)OrderEnum.OrderStatus.DONE)
                {
                    throw new BadRequestException("Order cannot cancel when the meal preparation had finished.");
                }

                if (order.OrderStatus == (int)OrderEnum.OrderStatus.CANCELLED)
                {
                    throw new BadRequestException("Order has already been canceled.");
                }

                var walletCustomer = await _unitOfWork.WalletDAO.GetWalletByAccountIdAsync(order.Customer.AccountID);

                // Seeding Owner store have id = 1
                var walletOwner = await _unitOfWork.WalletDAO.GetWalletByAccountIdAsync((int)RoleEnum.Role.OWNERSTORE);
                #endregion

                #region Count amount of product order detail
                var orderedProducts = new List<Product>();
                foreach (var orderDetail in order.OrderDetails)
                {
                    var productMeals = await _unitOfWork.ProductMealDAO.GetProductMealByMealIdAsync(orderDetail.MealID);
                    foreach (var productMeal in productMeals)
                    {
                        if (orderedProducts.Count > 0)
                        {
                            if (orderedProducts.FirstOrDefault(o => o.ID == productMeal.ProductID) == null)
                            {
                                orderedProducts.Add(new Product
                                {
                                    ID = productMeal.ProductID,
                                    Total = productMeal.Amount * orderDetail.Quantity
                                });
                            }
                            else
                            {
                                orderedProducts.FirstOrDefault(o => o.ID == productMeal.ProductID).Total += (productMeal.Amount * orderDetail.Quantity);
                            }
                        }
                        else
                        {
                            orderedProducts.Add(new Product
                            {
                                ID = productMeal.ProductID,
                                Total = productMeal.Amount * orderDetail.Quantity
                            });
                        }
                    }
                }
                #endregion

                #region Plus amount of product
                //// Plus amount to total of product (Status = 1(STOCKING) if total > 0)
                foreach (var orderdProduct in orderedProducts)
                {
                    Product product = await _unitOfWork.ProductDAO.GetProductByIdAsync(orderdProduct.ID);
                    product.Total += orderdProduct.Total;
                    if (product.Total > 0 && product.Status == (int)ProductEnum.Status.OUTOFSTOCK)
                    {
                        product.Status = (int)ProductEnum.Status.STOCKING;

                        foreach (var productMeal in product.ProductMeals)
                        {
                            if (productMeal.Meal.Status == (int)MealEnum.Status.OUTOFSTOCK)
                            {
                                productMeal.Meal.Status = (int)MealEnum.Status.STOCKING;
                            }
                        }
                        _unitOfWork.ProductDAO.UpdateProduct(product);
                    }
                }
                #endregion

                #region Update order and save order transaction
                var orderTransaction = new OrderLog
                {
                    PaymentTime = DateTime.Now,
                    Status = (int)OrderLogEnum.Status.CANCELLED
                };


                order.OrderStatus = (int)OrderEnum.OrderStatus.CANCELLED;
                order.OrderTransactions.Add(orderTransaction);
                _unitOfWork.OrderDAO.UpdateOrder(order);
                #endregion

                #region Save wallet and wallet transaction
                // Minus balance owner waller (transactiontype = SEND)
                WalletTransaction walletTransactionOwner = new WalletTransaction
                {
                    RechargeID = DateTime.Now.Ticks.ToString(),
                    RechargeTime = DateTime.Now,
                    Amount = order.Total,
                    Content = $"Refund for canceled [order - {orderId}] of [{customer.Account.Email} - {customer.FullName}]",
                    TransactionType = WalletTransactionEnum.TransactionType.SEND.ToString(),
                    RechargeStatus = (int)WalletTransactionEnum.RechangeStatus.SUCCESSED,
                    Wallet = walletOwner
                };

                await _unitOfWork.WalletTransactionDAO.CreateWalletTransactionAsync(walletTransactionOwner);

                walletOwner.Balance -= order.Total;
                _unitOfWork.WalletDAO.UpdateWallet(walletOwner);

                // Plus balance customer wallet (transactiontype = RECEIVE)
                WalletTransaction walletTransactionCustomer = new WalletTransaction
                {
                    RechargeID = DateTime.Now.Ticks.ToString(),
                    RechargeTime = DateTime.Now,
                    Amount = order.Total,
                    Content = $"Receive money from canceled [order - {orderId}].",
                    TransactionType = WalletTransactionEnum.TransactionType.RECEIVE.ToString(),
                    RechargeStatus = (int)WalletTransactionEnum.RechangeStatus.SUCCESSED,
                    Wallet = walletCustomer
                };

                await _unitOfWork.WalletTransactionDAO.CreateWalletTransactionAsync(walletTransactionCustomer);

                walletCustomer.Balance += order.Total;
                _unitOfWork.WalletDAO.UpdateWallet(walletCustomer);
                #endregion

                await _unitOfWork.CommitAsync();
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

        #region Get orders
        public async Task<List<GetOrderResponse>> GetOrdersAsync()
        {
            try
            {
                var orders = await _unitOfWork.OrderDAO.GetOrdersAsync();
                return _mapper.Map<List<GetOrderResponse>>(orders);
            }
            catch (Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }
        }
        #endregion

        #region Get order detail
        public async Task<GetOrderResponse> GetOrderDetailAsync(int id)
        {
            try
            {
                var order = await _unitOfWork.OrderDAO.GetOrderByIdAsync(id);
                if (order == null)
                {
                    throw new NotFoundException("Order does not exist.");
                }

                return _mapper.Map<GetOrderResponse>(order);
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
        #endregion

        public async Task<GetOrderResponse> GetCustomerOrder(int customerId, int orderId)
        {
            try
            {
                var customer = await _unitOfWork.CustomerDAO.GetCustomerByAccountIdAsync(customerId);
                if (customer == null)
                {
                    throw new NotFoundException("Customer does not exist.");
                }


                if (customer.Orders.SingleOrDefault(o => o.ID == orderId) == null)
                {
                    throw new NotFoundException("Customer does not have this order.");
                }

                return this._mapper.Map<GetOrderResponse>(customer.Orders.SingleOrDefault(x => x.ID == orderId));
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

        public async Task<List<GetOrderResponse>> GetCustomerOrders(int customerId)
        {
            try
            {
                var customer = await _unitOfWork.CustomerDAO.GetCustomerByAccountIdAsync(customerId);
                if (customer == null)
                {
                    throw new NotFoundException("Customer does not exist.");
                }
                List<Order> orders = await this._unitOfWork.OrderDAO.GetCustomerOrders(customerId);
                return this._mapper.Map<List<GetOrderResponse>>(orders);

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
    }
}
