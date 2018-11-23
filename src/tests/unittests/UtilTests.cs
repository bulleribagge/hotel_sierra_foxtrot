using Newtonsoft.Json;
using swingtrader;
using swingtrader.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace unittests
{
    public class UtilTests
    {
        [Fact]
        public void TestBollingerCalculation()
        {
            // 0.00008365, 0.00008451, 0.00008535

            var klines = new List<Kline>
            {
                new Kline { Close = 0.00008530m },
                new Kline { Close = 0.00008520m },
                new Kline { Close = 0.00008508m },
                new Kline { Close = 0.00008486m },
                new Kline { Close = 0.00008544m },
                new Kline { Close = 0.00008504m },
                new Kline { Close = 0.00008500m },
                new Kline { Close = 0.00008493m },
                new Kline { Close = 0.00008486m },
                new Kline { Close = 0.00008473m },
                new Kline { Close = 0.00008422m },
                new Kline { Close = 0.00008427m },
                new Kline { Close = 0.00008421m },
                new Kline { Close = 0.00008413m },
                new Kline { Close = 0.00008442m },
                new Kline { Close = 0.00008391m },
                new Kline { Close = 0.00008350m },
                new Kline { Close = 0.00008363m },
                new Kline { Close = 0.00008380m },
                new Kline { Close = 0.00008400m },
                new Kline { Close = 0.00008412m }
            };

            var boll = Util.CalculateBollinger(klines, 21, 1.5);

            Assert.Equal(0.00008366, (double)boll.LowBand, 8);
            Assert.Equal(0.00008451, (double)boll.MiddleBand, 8);
            Assert.Equal(0.00008536, (double)boll.UpperBand, 8);
        }

        [Fact]
        public void TestRounding()
        {
            Assert.Equal(0.05499M, Util.RoundDownToPrecision(0.054999M, 5));
            Assert.Equal(0.1M, Util.RoundDownToPrecision(0.1M, 1));
            Assert.Equal(1M, Util.RoundDownToPrecision(1.342666549M, 0));
            Assert.Equal(10M, Util.RoundDownToPrecision(14.543M, -1));
        }

        [Fact]
        public void TestOrderQuantityCalculation()
        {
            var orders = new List<OrderBookOrder>
            {
                new OrderBookOrder { Price = 1, Quantity = 1, Type = Enums.OrderBookType.ASK },
                new OrderBookOrder { Price = 2, Quantity = 1, Type = Enums.OrderBookType.ASK },
                new OrderBookOrder { Price = 3, Quantity = 1, Type = Enums.OrderBookType.ASK },
                new OrderBookOrder { Price = 4, Quantity = 1, Type = Enums.OrderBookType.ASK },
                new OrderBookOrder { Price = 5, Quantity = 1, Type = Enums.OrderBookType.ASK },
                new OrderBookOrder { Price = 6, Quantity = 1, Type = Enums.OrderBookType.ASK },
                new OrderBookOrder { Price = 7, Quantity = 1, Type = Enums.OrderBookType.ASK }
            };

            Assert.Equal(5, Util.GetBestPriceForQuantity(orders, 5));
            Assert.Equal(1, Util.GetBestPriceForQuantity(orders, 1));
        }

        [Fact]
        public void TestGetAveragePriceOfFills()
        {
            var fills = new List<Fill>
            {
                new Fill { Price = 20, Quantity = 5 },
                new Fill { Price = 22, Quantity = 10 },
                new Fill { Price = 25, Quantity = 10 }
            };

            Assert.Equal(22.8M, Util.GetAveragePriceOfFills(fills));
        }
    }
}
