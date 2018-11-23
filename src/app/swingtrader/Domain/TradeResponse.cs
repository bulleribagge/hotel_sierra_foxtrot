using Newtonsoft.Json;
using swingtrader.Domain;
using System;
using System.Collections.Generic;

namespace swingtrader.Domain
{
    public class TradeResponse
    {
        public string Symbol { get; set; }

        public long OrderId { get; set; }

        public string ClientOrderId { get; set; }

        [JsonConverter(typeof(UnixMillisecondTimestampConverter))]
        public DateTime TransactTime { get; set; }

        public decimal OriginalQuantity { get; set; }

        public decimal ExecutedQuantity { get; set; }

        public Enums.TradeStatus Status { get; set; }

        public Enums.TradeType Type { get; set; }

        public Enums.OrderSides Side { get; set; }

        public List<Fill> Fills { get; set; }
    }
}
