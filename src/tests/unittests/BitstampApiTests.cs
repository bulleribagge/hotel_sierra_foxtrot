using Flurl.Http.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using swingtrader;
using swingtrader.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace unittests
{
    public class BitstampApiTests
    {
        [Fact]
        public async void ReloadAndGetRate()
        {
            TestConfiguration config = new TestConfiguration()
            {
                Settings = new Dictionary<string, string>
                {
                    { "Bitstamp:Url", "https://www.bitstamp.net/" }
                }
            };

            var bitstampService = new BitstampApiService(config);

            using (var httpTest = new HttpTest())
            {
                string respJson = @"{
                    'high': '9355.42',
                    'last': '8904.97',
                    'timestamp': '1525181567',
                    'bid': '8904.97',
                    'vwap': '9057.46',
                    'volume': '12603.15581449',
                    'low': '8815.91',
                    'ask': '8904.99',
                    'open': '9243.51'
                }";

                httpTest.RespondWith(respJson);
                bitstampService.AddSymbol("btc");
                await bitstampService.RefreshRates();
                var res = bitstampService.GetRateForSymbol("btc");
                Assert.Equal(8904.97m, res);
            }
        }

        [Fact]
        public async void ReloadAndGetRateWhenBitstampIsDownAndThenUpAgain()
        {
            TestConfiguration config = new TestConfiguration()
            {
                Settings = new Dictionary<string, string>
                {
                    { "Bitstamp:Url", "https://www.bitstamp.net/" }
                }
            };

            var bitstampService = new BitstampApiService(config);

            using (var httpTest = new HttpTest())
            {
                string respJson = @"{
                    'high': '9355.42',
                    'last': '8904.97',
                    'timestamp': '1525181567',
                    'bid': '8904.97',
                    'vwap': '9057.46',
                    'volume': '12603.15581449',
                    'low': '8815.91',
                    'ask': '8904.99',
                    'open': '9243.51'
                }";

                httpTest.RespondWith("error", status: 500)
                    .RespondWith(respJson);
                bitstampService.AddSymbol("btc");

                await bitstampService.RefreshRates();
                var res = bitstampService.GetRateForSymbol("btc");
                Assert.Equal(0m, res);

                await bitstampService.RefreshRates();
                res = bitstampService.GetRateForSymbol("btc");
                Assert.Equal(8904.97m, res);
            }
        }

        [Fact]
        public async void ReloadAndGetOldRateWhenBitstampIsUpAndThenDown()
        {
            TestConfiguration config = new TestConfiguration()
            {
                Settings = new Dictionary<string, string>
                {
                    { "Bitstamp:Url", "https://www.bitstamp.net/" }
                }
            };

            var bitstampService = new BitstampApiService(config);

            using (var httpTest = new HttpTest())
            {
                string respJson = @"{
                    'high': '9355.42',
                    'last': '8904.97',
                    'timestamp': '1525181567',
                    'bid': '8904.97',
                    'vwap': '9057.46',
                    'volume': '12603.15581449',
                    'low': '8815.91',
                    'ask': '8904.99',
                    'open': '9243.51'
                }";

                httpTest.RespondWith(respJson)
                    .RespondWith("error", status: 500);
                    
                bitstampService.AddSymbol("btc");

                await bitstampService.RefreshRates();
                var res = bitstampService.GetRateForSymbol("btc");
                Assert.Equal(8904.97m, res);

                await bitstampService.RefreshRates();
                res = bitstampService.GetRateForSymbol("btc");
                Assert.Equal(8904.97m, res);
            }
        }

        [Fact]
        public async void ReloadAndAddTwoOfSameSymbolShouldOnlygetOneGetRate()
        {
            TestConfiguration config = new TestConfiguration()
            {
                Settings = new Dictionary<string, string>
                {
                    { "Bitstamp:Url", "https://www.bitstamp.net/" }
                }
            };

            var bitstampService = new BitstampApiService(config);

            using (var httpTest = new HttpTest())
            {
                string respJson = @"{
                    'high': '9355.42',
                    'last': '8904.97',
                    'timestamp': '1525181567',
                    'bid': '8904.97',
                    'vwap': '9057.46',
                    'volume': '12603.15581449',
                    'low': '8815.91',
                    'ask': '8904.99',
                    'open': '9243.51'
                }";

                httpTest.RespondWith(respJson);
                bitstampService.AddSymbol("btc");
                bitstampService.AddSymbol("btc");
                await bitstampService.RefreshRates();
                var res = bitstampService.GetRateForSymbol("btc");
                Assert.Equal(8904.97m, res);
                httpTest.ShouldHaveCalled(config.Settings["Bitstamp:Url"] + "api/v2/ticker/btcusd")
                    .Times(1);
            }
        }
    }
}
