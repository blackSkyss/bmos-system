using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMOS.DAL.Enums;
using BMOS.DAL.Infrastructures;
using BMOS.DAL.Models;
using Quartz;

namespace BMOS.BAL.ScheduleJob
{
    public class MealStatusUpdateJob : IJob
    {
        private UnitOfWork _unitOfWork;

        public MealStatusUpdateJob(IUnitOfWork unitOfWork)
        {
            _unitOfWork = (UnitOfWork)unitOfWork;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            await UpdateMealStatus();
            //return Task.CompletedTask;
        }

        private async Task UpdateMealStatus()
        {
            try
            {
                List<Meal> meals = await _unitOfWork.MealDAO.GetMealsAsync();
                bool checkChange = false;

                foreach (var meal in meals)
                {
                    int statusOfMeal = await CheckStatusOfMeal(meal.ID);
                    if (statusOfMeal != meal.Status)
                    {
                        checkChange = true;
                        meal.Status = statusOfMeal;
                        _unitOfWork.MealDAO.UpdateMeal(meal);
                    }
                }

                if (checkChange)
                {
                    await _unitOfWork.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task<int> CheckStatusOfMeal(int mealID)
        {
            try
            {
                List<ProductMeal> productMeals = await _unitOfWork.ProductMealDAO.GetListProductMealByMealID(mealID);
                foreach (var productMeal in productMeals)
                {
                    if (productMeal.Amount > productMeal.Product.Total)
                    {
                        return (int)MealEnum.Status.OUTOFSTOCK;
                    }
                    if (productMeal.Product.ExpiredDate <= DateTime.Now)
                    {
                        return (int)MealEnum.Status.INACTIVE;
                    }
                }
                return (int)MealEnum.Status.STOCKING;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
