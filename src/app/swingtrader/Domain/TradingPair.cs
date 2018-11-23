using Amazon.DynamoDBv2.DataModel;
using System;

namespace swingtrader.Domain
{
    [DynamoDBTable("TradingPair")]
    public class TradingPair
    {
        public string From { get; set; }
        public string To { get; set; }
        [DynamoDBIgnore]
        public string Pair { get { return From + To; } set { } }
        public string Interval { get; set; }
        public string Id { get { return Pair + Interval; } set { } }
        public int NumTrades { get; set; }
        public Enums.OrderSides LastAction { get; set; }
        public decimal LastRate { get; set; }
        public DateTime LastCheck { get; set; }
        public int PriceStepSize { get; set; }
        public int LotStepSize { get; set; }
        public decimal ProfitPercent { get; set; }
        public decimal Profit { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal LastBuyPrice { get; set; }
        public decimal UnitsInvested { get; set; }
        public bool BuyingEnabled { get; set; }

        public TradingPair()
        {

        }
    }
}
