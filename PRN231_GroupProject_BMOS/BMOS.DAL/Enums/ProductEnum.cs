using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.DAL.Enums
{
    public static class ProductEnum
    {
        public enum Status
        {
            OUTOFSTOCK = 0,
            STOCKING = 1,
            INACTIVE = 2,
        }
    }
}
