using System.Collections.Generic;

namespace swingtrader.Domain
{
    public class GetOrderBookResponse
    {
        public int LastUpdateId { get; set; }
        public List<OrderBookOrder> Bids { get; set; }
        public List<OrderBookOrder> Asks { get; set; }

        public GetOrderBookResponse(dynamic obj)
        {
            LastUpdateId = obj.lastUpdateId;
            Bids = new List<OrderBookOrder>();
            Asks = new List<OrderBookOrder>();

            foreach (var bid in obj.bids)
            {
                Bids.Add(new OrderBookOrder
                {
                    Price = bid[0],
                    Quantity = bid[1],
                    Type = Enums.OrderBookType.BID
                });
            }

            foreach (var ask in obj.asks)
            {
                Asks.Add(new OrderBookOrder
                {
                    Price = ask[0],
                    Quantity = ask[1],
                    Type = Enums.OrderBookType.ASK
                });
            }
        }
    }
}
