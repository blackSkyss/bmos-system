using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMOS.BAL.DTOs.Customers;
using BMOS.BAL.DTOs.OrderDetails;
using BMOS.DAL.Models;

namespace BMOS.BAL.DTOs.Orders
{
    public class GetOrdersResponse
    {
        [Key]
        public int ID { get; set; }
        public DateTime OrderedDate { get; set; }
        public decimal Total { get; set; }
        public int OrderStatus { get; set; }
        public  GetCustomerResponse Customer { get; set; }
        public  ICollection<GetOrderDetailResponse> OrderDetails { get; set; }
        public  ICollection<OrderLog> OrderTransactions { get; set; }
    }
}
