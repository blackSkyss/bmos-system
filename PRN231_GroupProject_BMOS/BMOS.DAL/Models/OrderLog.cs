using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.DAL.Models
{
    public class OrderLog
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public DateTime PaymentTime { get; set; }
        public int Status { get; set; }
        public virtual Order Order { get; set; }
    }
}
