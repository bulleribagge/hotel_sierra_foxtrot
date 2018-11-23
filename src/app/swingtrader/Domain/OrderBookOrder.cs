using static swingtrader.Domain.Enums;

namespace swingtrader.Domain
{
    public class OrderBookOrder
    {
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public OrderBookType Type { get; set; }
    }
}
