using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.DAL.Models
{
    public class Customer
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
        public virtual Account Account { get; set; }
        public virtual ICollection<Order> Orders { get; set; }

    }
}
