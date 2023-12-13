using BMOS.BAL.DTOs.Staffs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.DTOs.Accounts
{
    public class PostAccountRequest
    {
        [Key]
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }
}
