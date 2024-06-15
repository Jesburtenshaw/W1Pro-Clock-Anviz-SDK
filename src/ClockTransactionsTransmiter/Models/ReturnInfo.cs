using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockTransactionsTransmiter.Models
{
    public class ReturnInfo
    {
        public string Code { get; set; }
        public string Tip { get; set; }
    }

    public class ReturnInfo<T> : ReturnInfo
    {
        public T Body { get; set; }
    }
}