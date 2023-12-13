using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.DTOs.Orders
{
    public class PostOrderRequest
    {
        [Key]
        public string Email { get; set; }
        public List<PostListMealOrderRequest> Meals { get; set; }
    }
}
