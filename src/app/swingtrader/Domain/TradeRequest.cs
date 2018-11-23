using System;
using static swingtrader.Domain.Enums;

namespace swingtrader.Domain
{
    public class TradeRequest
    {
        public long Timestamp { get; set; }
        public string Symbol { get; set; }
        public OrderSides Side { get; set; }
        public TradeType Type { get; set; }
        public Decimal Quantity { get; set; }
        public Decimal Price { get; set; }
        public OrderResponseType NewOrderRespType { get; set; }
        public TimeInForce TimeInForce { get; set; }
    }
}
