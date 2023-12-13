using BMOS.DAL.DBContext;
using BMOS.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.DAL.DAOs
{
    public class ProductImageDAO
    {
        private BMOSDbContext _dbContext;
        public ProductImageDAO(BMOSDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        #region Get ProductImage by ImageId
        public ProductImage GetProductByImageId(string id)
        {
            try
            {
                return _dbContext.ProductImages.SingleOrDefault(p => p.ImageID.Equals(id));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region DeleteProduct
        public void DeleteProductImage(ProductImage productImage)
        {
            try
            {
                this._dbContext.ProductImages.Remove(productImage);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion
    }
}
