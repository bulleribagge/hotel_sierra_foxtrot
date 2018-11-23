using Newtonsoft.Json;
using System;

namespace swingtrader.Domain
{
    public class Ticker
    {
        [JsonProperty(PropertyName = "symbol")]
        public string Symbol { get; set; }
        [JsonProperty(PropertyName = "price")]
        public Decimal Price { get; set; }
    }
}
