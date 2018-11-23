using System;
using System.Collections.Generic;
using System.Text;

namespace swingtrader.Domain
{
    public class GetOrderBookRequest
    {
        public string Symbol { get; set; }
        public int? Limit { get; set; }
    }
}
