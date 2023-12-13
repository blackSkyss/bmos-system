using BMOS.DAL.DBContext;
using BMOS.DAL.Enums;
using BMOS.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.DAL.DAOs
{
    public class OrderDAO
    {
        private BMOSDbContext _dbContext;
        public OrderDAO(BMOSDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<List<Order>> GetOrdersWithOverNewOrderStatus()
        {
            try
            {
                return await this._dbContext.Orders.Include(o => o.Customer)
                                                   .Include(o => o.OrderDetails)
                                                   .Include(o => o.OrderTransactions)
                                                   .Where(x => x.OrderedDate.AddDays(3).Date <= DateTime.Today.Date && x.OrderStatus == (int)OrderEnum.OrderStatus.NEWORDER)
                                                   .ToListAsync();
            }catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        // get all orders
        public async Task<List<Order>> GetCustomerOrders(int customerId)
        {
            try
            {
                return await _dbContext.Orders.Include(o => o.Customer).ThenInclude(c => c.Account).ThenInclude(x => x.Role)
                                              .Include(x => x.OrderDetails).ThenInclude(od => od.Meal).ThenInclude(x => x.MealImages)
                                              .Include(x => x.OrderDetails).ThenInclude(od => od.Meal).ThenInclude(x => x.ProductMeals).ThenInclude(x => x.Product).ThenInclude(x => x.ProductImages)
                                              .Include(x => x.OrderTransactions)
                                              .Where(o => o.Customer.AccountID == customerId).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        
    

        #region Get orders
        public async Task<List<Order>> GetOrdersAsync()
        {
            try
            {
                return await _dbContext.Orders.Include(o => o.Customer).ThenInclude(c => c.Account).ThenInclude(x => x.Role)
                                              .Include( o => o.OrderDetails).ThenInclude(odd => odd.Meal).ThenInclude(m => m.ProductMeals).ThenInclude(proMeals => proMeals.Product).ThenInclude(p => p.ProductImages)
                                              .Include( o => o.OrderDetails).ThenInclude(odd => odd.Meal).ThenInclude(m => m.MealImages)
                                              .Include(o => o.OrderTransactions)
                                              .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Get order by id
        public async Task<Order> GetOrderByIdAsync(int id)
        {
            try
            {
                return await _dbContext.Orders.Include(o => o.Customer).ThenInclude(c => c.Account).ThenInclude(x => x.Role)
                                              .Include(o => o.OrderDetails).ThenInclude(odd => odd.Meal).ThenInclude(m => m.ProductMeals).ThenInclude(proMeals => proMeals.Product).ThenInclude(p => p.ProductImages)
                                              .Include(o => o.OrderDetails).ThenInclude(odd => odd.Meal).ThenInclude(m => m.MealImages)
                                              .Include(o => o.OrderTransactions)
                                              .SingleOrDefaultAsync(o => o.ID == id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Create order
        public async Task CreateOrderAsync(Order order)
        {
            try
            {
                await this._dbContext.Orders.AddAsync(order);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Update order
        public void UpdateOrder(Order order)
        {
            try
            {
                this._dbContext.Entry<Order>(order).State = EntityState.Modified;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion
    }
}
