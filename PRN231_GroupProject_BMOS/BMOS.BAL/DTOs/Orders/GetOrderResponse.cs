using BMOS.BAL.DTOs.Customers;
using BMOS.BAL.DTOs.OrderDetails;
using BMOS.BAL.DTOs.OrderTransactions;
using BMOS.DAL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.DTOs.Orders
{
    public class GetOrderResponse
    {
        [Key]
        public int ID { get; set; }
        public DateTime OrderedDate { get; set; }
        public decimal Total { get; set; }
        public int OrderStatus { get; set; }
        public virtual GetCustomerResponse Customer { get; set; }
        public ICollection<GetOrderDetailResponse> OrderDetails { get; set; }
        public ICollection<GetOrderLogResponse> OrderTransactions { get; set; }
    }
}
