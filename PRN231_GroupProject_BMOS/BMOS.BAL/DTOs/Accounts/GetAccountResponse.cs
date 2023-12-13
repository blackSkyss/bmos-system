using BMOS.BAL.DTOs.Roles;
using BMOS.BAL.DTOs.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.DTOs.Accounts
{
    public class GetAccountResponse
    {
        [Key]
        public int ID { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public bool Status { get; set; }
        public string RoleName { get; set; }
    }
}
