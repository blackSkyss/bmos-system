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
    public class DashBoardDAO
    {
        private BMOSDbContext _dbContext;
        public DashBoardDAO(BMOSDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            try
            {
                return await _dbContext.Products.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Meal>> GetMealsAsync()
        {
            try
            {
                return await _dbContext.Meals.Include(m => m.ProductMeals)
                                             .ThenInclude(p => p.Product).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Order>> GetOrdersAsync()
        {
            try
            {
                return await this._dbContext.Orders.Include(x => x.OrderDetails).ThenInclude(x => x.Meal).ThenInclude(x => x.ProductMeals)
                                                   .ThenInclude(x => x.Product).Where(x => x.OrderStatus == (int)OrderEnum.OrderStatus.NEWORDER 
                                                                  || x.OrderStatus == (int)OrderEnum.OrderStatus.DONE).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        
        public async Task<List<Order>> GetDoneOrdersAsync()
        {
            try
            {
                return await this._dbContext.Orders.Include(x => x.OrderDetails).ThenInclude(x => x.Meal).ThenInclude(x => x.ProductMeals)
                                                   .ThenInclude(x => x.Product).Where(x =>  x.OrderStatus == (int)OrderEnum.OrderStatus.DONE)
                                                   .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
