using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.DAL.Enums
{
    public class OrderLogEnum
    {
        public enum Status
        {
            PAID = 0,
            PROCESSING = 1,
            DONE = 2,
            CANCELLED = 3,
        }
    }
}
