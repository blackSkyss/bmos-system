using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.DAL.Models
{
    public class Token
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string JwtID { get; set; }
        public string RefreshToken { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiredDate { get; set; }
        public bool IsUsed { get; set; }
        public bool IsRevoked { get; set; }
        public virtual Account Account { get; set; }
    }
}
