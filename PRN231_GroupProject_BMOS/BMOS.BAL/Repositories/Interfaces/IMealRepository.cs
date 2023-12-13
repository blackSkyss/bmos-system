using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMOS.BAL.DTOs.FireBase;
using BMOS.BAL.DTOs.Meals;
using Microsoft.AspNetCore.Http;

namespace BMOS.BAL.Repositories.Interfaces
{
    public interface IMealRepository
    {
        public Task<List<GetMealResponse>> GetMealsAsync();
        public Task<GetMealResponse> GetMealByID(int id);
        public Task<List<GetMealResponse>> GetAvailableMeals();
        public Task<GetMealResponse> GetAvailableMeal(int mealId);
        public Task<GetMealResponse> CreateMeal(FireBaseImage fireBaseImage, PostMealRequest newMeal, HttpContext httpContext);
        public Task<GetMealResponse> UpdateMeal(int ID, FireBaseImage fireBaseImage, UpdateMealRequest updateMealRequest, HttpContext httpContext);
        public Task DeleteMeal(int ID, HttpContext httpContext);

    }
}
