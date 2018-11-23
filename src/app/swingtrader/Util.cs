using swingtrader.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace swingtrader
{
    public static class Util 
    {
        public static Bollinger CalculateBollinger(List<Kline> klines, int period, double deviationMultiplier)
        {
            klines.Reverse();
            klines = klines.Take(period).ToList();

            double sum = 0;
            double deviationSum = 0;
            double mean = 0;
            double standardDeviation = 0;

            sum = klines.Sum(x => (double)x.Close);
            mean = sum / period;
            deviationSum = klines.Sum(x => Math.Pow(((double)x.Close - mean), 2));
            standardDeviation = Math.Sqrt(deviationSum / period);

            return new Bollinger
            {
                LowBand = (decimal)(mean - (deviationMultiplier * standardDeviation)),
                MiddleBand = (decimal)mean,
                UpperBand = (decimal)(mean + (deviationMultiplier * standardDeviation))
            };
        }

        public static decimal RoundDownToPrecision(decimal val, int precision)
        {
            decimal adjustment = (decimal)Math.Pow(10, precision);
            return Math.Floor(val * adjustment) / adjustment;
        }

        public static decimal GetPriceDiffPercent(decimal a, decimal b)
        {
            return (Math.Abs(a - b) / ((a + b) / 2)) * 100;
        }

        public static long GetTimestamp()
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public static decimal GetBestPriceForQuantity(List<OrderBookOrder> orders, decimal quantity)
        {
            var sum = 0M;
            foreach (var order in orders)
            {
                sum += order.Quantity;
                if(sum >= quantity)
                {
                    return order.Price;
                }
            }

            return 0M;
        }

        public static decimal GetAveragePriceOfFills(List<Fill> fills)
        {
            var quantitySum = fills.Sum(x => x.Quantity);
            var priceProduct = 0M;

            foreach (var fill in fills)
            {
                priceProduct += fill.Price * fill.Quantity;
            }

            return priceProduct / quantitySum;
        }
    }
}
