using BMOS.BAL.DTOs.OrderDetails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMOS.BAL.DTOs.OrderDetails;

namespace BMOS.BAL.Repositories.Interfaces
{
    public interface IOrderDetailRepository
    {
        public Task<IEnumerable<GetOrderDetailResponse>> GetOrderDetailsByOrderIdAsync(int? id);
        public Task<List<GetOrderDetailResponse>> GetOrderDetailsByOrderID(int  orderID);
    }
}
