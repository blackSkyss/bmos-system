using BMOS.BAL.DTOs.Accounts;
using BMOS.DAL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.DTOs.Staffs
{
    public class GetStaffResponse
    {
        [Key]
        public int AccountID { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Avatar { get; set; }
        public string AvatarID { get; set; }
        public bool Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public string IdentityNumber { get; set; }
        public DateTime RegisteredDate { get; set; }
        public DateTime? QuitDate { get; set; }
        public GetAccountResponse Account { get; set; }
    }
}
