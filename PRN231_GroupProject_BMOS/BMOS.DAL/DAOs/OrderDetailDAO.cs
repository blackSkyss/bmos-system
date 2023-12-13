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
    public class OrderDetailDAO
    {
        private BMOSDbContext _dbContext;
        public OrderDetailDAO(BMOSDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        //get order details by order id
        public async  Task<List<OrderDetail>> GetOrderDetailsByOrderID(int orderID)
        {
            try
            {
                List<OrderDetail> orderDetails = await _dbContext.OrderDetails.Where(x => x.OrderID == orderID).ToListAsync();
                return orderDetails;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        #region Get order detail by orderId
        public async Task<IEnumerable<OrderDetail>> GetOrderDetailByOrderIdAsync(int id)
        {
            try
            {
                return await _dbContext.OrderDetails.Where(od => od.OrderID == id)
                                                    .Include(od => od.Meal)
                                                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion
    }
}
