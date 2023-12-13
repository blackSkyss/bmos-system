using AutoMapper;
using BMOS.BAL.DTOs.ProductMeals;
using BMOS.BAL.DTOs.Products;
using BMOS.BAL.Helpers;
using BMOS.BAL.Repositories.Interfaces;
using BMOS.DAL.Infrastructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Repositories.Implementations
{
    public class ProductMealRepository: IProductMealRepository
    {
        private UnitOfWork _unitOfWork;
        private IMapper _mapper;
        public ProductMealRepository(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = (UnitOfWork)unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<GetProductByProductMealsResponse>> GetProductsByMealID(int mealID)
        {
            try
            {
                var productMeals = await this._unitOfWork.ProductMealDAO.GetProductsByMealID(mealID);
                var result = _mapper.Map<List<GetProductByProductMealsResponse>>(productMeals);
                return result;
            }
            catch (Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }
        }
    }
}
