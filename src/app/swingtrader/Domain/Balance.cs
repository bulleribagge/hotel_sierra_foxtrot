using Newtonsoft.Json;
using System;

namespace swingtrader.Domain
{
    public class Balance
    {
        [JsonProperty(PropertyName = "asset")]
        public string Asset { get; set; }
        [JsonProperty(PropertyName = "free")]
        public Decimal Free { get; set; }
        [JsonProperty(PropertyName = "locked")]
        public Decimal Locked { get; set; }
        public decimal Total { get { return Locked + Free; } }
    }
}
