using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.DAL.Models
{
    public class Staff
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
        public virtual Account Account { get; set; }
    }
}
