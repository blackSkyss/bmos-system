using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.DTOs.Customers
{
    public class UpdateCustomerRequest
    {
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string? PasswordHash { get; set; }
        public IFormFile? Avatar { get; set; }
        public bool Gender { get; set; }
        public DateTime BirthDate { get; set; }
    }
}
