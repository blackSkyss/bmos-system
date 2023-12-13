using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.DTOs.Accounts
{
    public class ChangePasswordRequest
    {
        public string OldPassWord { get; set; }
        public string NewPassWord { get; set; }
        public string ConFirmPassword { get; set; }
    }
}
