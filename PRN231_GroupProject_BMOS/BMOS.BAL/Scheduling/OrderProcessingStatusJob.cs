using BMOS.DAL.Enums;
using BMOS.DAL.Infrastructures;
using BMOS.DAL.Models;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Scheduling
{
    public class OrderProcessingStatusJob: IJob
    {
        private UnitOfWork _unitOfWork;
        public OrderProcessingStatusJob(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = (UnitOfWork)unitOfWork;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await UpdateOrderStatus();
        }

        private async Task UpdateOrderStatus()
        {
            try
            {
                List<Order> expiredOrders = await this._unitOfWork.OrderDAO.GetOrdersWithOverNewOrderStatus();
                if(expiredOrders != null && expiredOrders.Count > 0)
                {
                    // Seeding Owner store have id = 1
                    var walletOwner = await _unitOfWork.WalletDAO.GetWalletByAccountIdAsync((int)RoleEnum.Role.OWNERSTORE);
                    foreach (var order in expiredOrders)
                    {
                        var walletCustomer = await _unitOfWork.WalletDAO.GetWalletByAccountIdAsync(order.Customer.AccountID);
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
                            var orderTransaction = new OrderLog
                            {
                                PaymentTime = DateTime.Now,
                                Status = (int)OrderLogEnum.Status.CANCELLED
                            };


                            order.OrderStatus = (int)OrderEnum.OrderStatus.CANCELLED;
                            order.OrderTransactions.Add(orderTransaction);
                            _unitOfWork.OrderDAO.UpdateOrder(order);
                            WalletTransaction walletTransactionOwner = new WalletTransaction
                            {
                                RechargeID = DateTime.Now.Ticks.ToString(),
                                RechargeTime = DateTime.Now,
                                Amount = order.Total,
                                Content = $"Refund for canceled [order - {order.ID}] of [{order.Customer.Account.Email} - {order.Customer.FullName}] " +
                                         $"because the order is NEW ORDER status more than 3 days from ordered date.",
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
                                Content = $"Receive money from canceled [order - {order.ID}]. " +
                                          $"because the order is NEW ORDER status more than 3 days from ordered date.",
                                TransactionType = WalletTransactionEnum.TransactionType.RECEIVE.ToString(),
                                RechargeStatus = (int)WalletTransactionEnum.RechangeStatus.SUCCESSED,
                                Wallet = walletCustomer
                            };

                            await _unitOfWork.WalletTransactionDAO.CreateWalletTransactionAsync(walletTransactionCustomer);

                            walletCustomer.Balance += order.Total;
                            _unitOfWork.WalletDAO.UpdateWallet(walletCustomer);
                        }
                    }
                    await _unitOfWork.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
