namespace swingtrader.Domain
{
    public class Enums
    {
        public enum TradeType
        {
            LIMIT,
            MARKET,
            STOP_LOSS,
            STOP_LOSS_LIMIT,
            TAKE_PROFIT,
            TAKE_PROFIT_LIMIT,
            LIMIT_MAKER
        }

        public enum TradeStatus
        {
            NEW,
            PARTIALLY_FILLED,
            FILLED,
            CANCELED,
            PENDING_CANCEL,
            REJECTED,
            EXPIRED
        }

        public enum OrderSides
        {
            BUY,
            SELL
        }

        public enum OrderBookType
        {
            ASK,
            BID
        }

        public enum OrderResponseType
        {
            ACK,
            RESULT,
            FULL
        }

        public enum TimeInForce
        {
            GTC,
            IOC,
            FOK
        }
    }
}
