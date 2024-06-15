using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClockTransactionsTransmiter.Models
{
    public class PageInfo
    {
        public int MaxPage { get; set; }
    }

    public class PageInfo<T> : PageInfo
    {
        public List<T> Items { get; set; }
    }
}