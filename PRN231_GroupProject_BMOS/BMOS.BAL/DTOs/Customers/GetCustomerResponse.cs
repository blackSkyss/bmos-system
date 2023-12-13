using BMOS.BAL.DTOs.Accounts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.DTOs.Customers
{
    public class GetCustomerResponse
    {
        [Key]
        public int AccountID { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Avatar { get; set; }
        public bool Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public GetAccountResponse Account { get; set; }
    }
}
