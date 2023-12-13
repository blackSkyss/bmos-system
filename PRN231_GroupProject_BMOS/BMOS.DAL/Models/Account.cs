using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.DAL.Models
{
    public class Account
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public bool Status { get; set; }
        public virtual Role Role { get; set; }
        public virtual ICollection<Token> Tokens { get; set; }
    }
}
