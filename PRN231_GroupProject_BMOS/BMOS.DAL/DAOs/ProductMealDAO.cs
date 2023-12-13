using BMOS.DAL.DBContext;
using BMOS.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.DAL.DAOs
{
    public class ProductMealDAO
    {
        private BMOSDbContext _dbContext;
        public ProductMealDAO(BMOSDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        //get product by MealID
        public async Task<List<ProductMeal>> GetProductsByMealID(int mealID)
        {
            try
            {
                List<ProductMeal> productMeals = await _dbContext.ProductMeals.Where(p => p.MealID == mealID).Include(x => x.Product).ToListAsync();
                return productMeals;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        //get list product meal by meal id include product
        public async Task<List<ProductMeal>> GetListProductMealByMealID(int mealID)
        {
            try
            {
                List<ProductMeal> productMeals = await _dbContext.ProductMeals.Where(p => p.MealID == mealID).Include(x => x.Product).ToListAsync();
                return productMeals;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #region Get product meal by mealId
        public async Task<IEnumerable<ProductMeal>> GetProductMealByMealIdAsync(int id)
        {
            try
            {
                return await _dbContext.ProductMeals.Where(p => p.MealID == id).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Get product meal by productId
        public async Task<IEnumerable<ProductMeal>> GetProductMealByProductIdAsync(int id)
        {
            try
            {
                return await _dbContext.ProductMeals.Where(p => p.ProductID == id).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion
    }
}
