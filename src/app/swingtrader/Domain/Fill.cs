using Newtonsoft.Json;
using System;

namespace swingtrader.Domain
{
    public class Fill
    {
        public decimal Price { get; set; }
        [JsonProperty(PropertyName = "qty")]
        public decimal Quantity { get; set; }
        public decimal Commission { get; set; }
    }   
}
