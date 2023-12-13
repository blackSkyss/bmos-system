using BMOS.DAL.Enums;
using BMOS.DAL.Infrastructures;
using BMOS.DAL.Models;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.ScheduleJob
{
    public class ProductStatusUpdateJob: IJob
    {
        private UnitOfWork _unitOfWork;
        public ProductStatusUpdateJob(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = (UnitOfWork)unitOfWork;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await this.UpdateProductsStatus();
        }

        private async Task UpdateProductsStatus()
        {
            try
            {
                List<Product> expiredProducts = await this._unitOfWork.ProductDAO.GetExpiredProducts();
                if(expiredProducts != null && expiredProducts.Count > 0)
                {
                    foreach (var expiredProduct in expiredProducts)
                    {
                        expiredProduct.Status = (int)ProductEnum.Status.INACTIVE;
                        if (expiredProduct.ProductMeals != null)
                        {
                            foreach (var productMeal in expiredProduct.ProductMeals)
                            {
                                productMeal.Meal.Status = (int)MealEnum.Status.INACTIVE;
                            }
                        }
                        this._unitOfWork.ProductDAO.UpdateProduct(expiredProduct);
                    }
                    await this._unitOfWork.CommitAsync();
                }
            } catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
