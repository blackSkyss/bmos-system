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
    public class MealDAO
    {
        private BMOSDbContext _dbContext;
        public MealDAO(BMOSDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        // get all meals include productmeals and products
        public async Task<List<Meal>> GetMealsAsync()
        {
            try
            {
                List<Meal> meals = await _dbContext.Meals.Include(x => x.ProductMeals).ThenInclude(x => x.Product).ThenInclude(x => x.ProductImages)
                                                         .Include(x => x.MealImages).ToListAsync();
                return meals;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        
        // get meal by id
        public async Task<Meal> GetMealByID(int id)
        {
            try
            {
                return  await _dbContext.Meals.Include(x => x.ProductMeals).ThenInclude(x => x.Product).ThenInclude(x => x.ProductImages)
                                              .Include(x => x.MealImages).FirstOrDefaultAsync(x => x.ID == id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        //get available meals
        public async Task<List<Meal>> GetAvailableMeals()
        {
            try
            {
                List<Meal> meals = await _dbContext.Meals.Include(x => x.ProductMeals).ThenInclude(x => x.Product)
                                                            .Include(x => x.MealImages)
                                                            .Where(m => m.Status == (int)MealEnum.Status.STOCKING).ToListAsync();
                return meals;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        
        public async Task<Meal> GetAvailableMeal(int mealId)
        {
            try
            {
                Meal meal = await _dbContext.Meals.Include(x => x.ProductMeals).ThenInclude(x => x.Product).ThenInclude(x => x.ProductImages)
                                                            .Include(x => x.MealImages)
                                                            .SingleOrDefaultAsync(m => m.Status == (int)MealEnum.Status.STOCKING && m.ID == mealId);
                return meal;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
       

        //save change
        public void UpdateMealStatus(Meal meal)
        {
            try
            {
                this._dbContext.Entry<Meal>(meal).State = EntityState.Modified;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }   


        #region Get meal by id
        public async Task<Meal> GetMealByIdAsync(int id)
        {
            try
            {
                return await _dbContext.Meals.Include(m => m.ProductMeals)
                                             .ThenInclude(pm => pm.Product)
                                             .SingleOrDefaultAsync(m => m.ID == id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Update meal
        public void UpdateMeal(Meal meal)
        {
            try
            {
                this._dbContext.Entry<Meal>(meal).State = EntityState.Modified;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        //add meal
        public async Task AddMeal(Meal meal)
        {
            try
            {
                await _dbContext.Meals.AddAsync(meal);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
