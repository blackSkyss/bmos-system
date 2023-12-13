using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.DTOs.DashBoard
{
    public class GetMonthProfitResponse
    {
        public int Month { get; set; }
        public decimal Profits { get; set; }
    }
}
