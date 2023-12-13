using BMOS.BAL.DTOs.Orders;
using BMOS.DAL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.DTOs.OrderTransactions
{
    public class GetOrderLogResponse
    {
        [Key]
        public int ID { get; set; }
        public DateTime PaymentTime { get; set; }
        public int Status { get; set; }
    }
}
