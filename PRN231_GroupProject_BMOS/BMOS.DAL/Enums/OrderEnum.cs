using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.DAL.Enums
{
    public static class OrderEnum
    {
        public enum OrderStatus
        {
            NEWORDER = 0,
            PROCESSING = 1,
            DONE = 2,
            CANCELLED = 3,
        }
    }
}
