using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMOS.BAL.DTOs.ProductMeals;
using BMOS.BAL.DTOs.Products;

namespace BMOS.BAL.Repositories.Interfaces
{
    public interface IProductMealRepository
    {
        public Task<List<GetProductByProductMealsResponse>> GetProductsByMealID(int mealID);
    }
}
