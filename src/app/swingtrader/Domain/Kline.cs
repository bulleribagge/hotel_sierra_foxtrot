using Newtonsoft.Json.Linq;
using System;

namespace swingtrader.Domain
{
    public class Kline
    {
        public DateTime OpenTime { get; set; }
        public Decimal Open { get; set; }
        public Decimal High { get; set; }
        public Decimal Low { get; set; }
        public Decimal Close { get; set; }
        public Decimal Volume { get; set; }
        public DateTime CloseTime { get; set; }
        public int NumTrades { get; set; }
        public string Pair { get; set; }

        public Kline()
        {

        }

        public Kline(JToken res, string pair)
        {
            OpenTime = DateTimeOffset.FromUnixTimeMilliseconds(res[0].Value<long>()).LocalDateTime;
            Open = res[1].Value<Decimal>();
            High = res[2].Value<Decimal>();
            Low = res[3].Value<Decimal>();
            Close = res[4].Value<Decimal>();
            Volume = res[5].Value<Decimal>();
            CloseTime = DateTimeOffset.FromUnixTimeMilliseconds(res[6].Value<long>()).LocalDateTime;
            NumTrades = res[8].Value<int>();
            Pair = pair;
        }
    }
}
