using BMOS.DAL.DBContext;
using BMOS.DAL.Enums;
using BMOS.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.DAL.DAOs
{
    public class ProductDAO
    {
        private BMOSDbContext _dbContext;
        public ProductDAO(BMOSDbContext dbContext)
        {
            this._dbContext = dbContext;
        }


        //get products
        public async Task<List<Product>> GetProducts()
        {
            try
            {
                List<Product> products = await _dbContext.Products.Include(p => p.ProductImages)
                                                                  .Include(p => p.ProductMeals).ThenInclude(pm => pm.Meal).ThenInclude(m => m.MealImages)
                                                                  .ToListAsync();
                return products;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        
        public async Task<List<Product>> GetActiveProducts()
        {
            try
            {
                List<Product> products = await _dbContext.Products.Include(p => p.ProductImages)
                                                                  .Include(p => p.ProductMeals.Where(x => x.Meal.Status == (int)MealEnum.Status.STOCKING)).ThenInclude(pm => pm.Meal).ThenInclude(m => m.MealImages)
                                                                  .Where(x => x.Status == (int)ProductEnum.Status.STOCKING)
                                                                  .ToListAsync();
                return products;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<Product> GetActiveProduct(int productId)
        {
            try
            {
                Product products = await _dbContext.Products.Include(p => p.ProductImages)
                                                                  .Include(p => p.ProductMeals.Where(x => x.Meal.Status == (int)MealEnum.Status.STOCKING)).ThenInclude(pm => pm.Meal).ThenInclude(m => m.MealImages)
                                                                  .SingleOrDefaultAsync(x => x.Status == (int)ProductEnum.Status.STOCKING && x.ID == productId);
                return products;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Product>> GetExpiredProducts()
        {
            try
            {
                return await this._dbContext.Products.Include(x => x.ProductMeals).ThenInclude(x => x.Meal)
                                                     .Where(x => x.ExpiredDate.Date <= DateTime.Now.Date)
                                                     .ToListAsync();
            } catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task CreateNewProduct(Product product)
        {
            try
            {
                await this._dbContext.Products.AddAsync(product);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #region Get product by id
        public async Task<Product> GetProductByIdAsync(int id)
        {
            try
            {
                return await _dbContext.Products
                    .Include(p => p.ProductImages)
                    .Include(p => p.ProductMeals)
                    .ThenInclude(p => p.Meal)
                    .ThenInclude(m => m.MealImages)
                   . SingleOrDefaultAsync(p => p.ID == id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Update product
        public void UpdateProduct(Product product)
        {
            try
            {
                this._dbContext.Entry<Product>(product).State = EntityState.Modified;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region DeleteProduct
        public void DeleteProduct(Product product)
        {
            try
            {
                this._dbContext.Products.Remove(product);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion
    }
}
