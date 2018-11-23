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
    public class BinanceApiTests
    {
        [Fact]
        public async void GetOneKlineAndParseIt()
        {
            TestConfiguration config = new TestConfiguration()
            {
                Settings = new Dictionary<string, string>
                {
                    { "Binance:Url", "https://api.binance.com" }
                }
            };

            var binanceService = new BinanceApiService(config);

            using (var httpTest = new HttpTest())
            {
                object[,] respArr = new object[,]
                {
                    {
                        1523336400000,  //0
                        0.28505000m,    //1
                        0.28543000m,    //2
                        0.28477000m,    //3
                        0.28539000m,    //4
                        85.38400000m,   //5
                        1523337299999,  //6
                        24.34433168m,   //7
                        38,             //8
                        60.08500000m,   //9
                        17.13244429m,   //10
                        0
                    }
                };

                httpTest.RespondWithJson(respArr);

                var res = await binanceService.GetKlines(new GetKlinesRequest { Symbol = "LTCETH", Interval = Constants.INTERVAL_15m });

                Assert.Single(res);

                var kline = res[0];

                Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds((long)respArr[0, 0]).LocalDateTime, kline.OpenTime);

                Assert.Equal(respArr[0, 1], kline.Open);

                Assert.Equal(respArr[0, 2], kline.High);

                Assert.Equal(respArr[0, 3], kline.Low);

                Assert.Equal(respArr[0, 4], kline.Close);

                Assert.Equal(respArr[0, 5], kline.Volume);

                Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds((long)respArr[0, 6]).LocalDateTime, kline.CloseTime);

                Assert.Equal(respArr[0, 8], kline.NumTrades);
            }
        }

        [Fact]
        public async void GetMultipleKlinesAndParseThem()
        {
            TestConfiguration config = new TestConfiguration()
            {
                Settings = new Dictionary<string, string>
                {
                    { "Binance:Url", "https://api.binance.com" }
                }
            };

            var binanceService = new BinanceApiService(config);

            using (var httpTest = new HttpTest())
            {
                object[,] respArr = new object[,]
                {
                    {
                        1523385900000,
                        0.27829000m,
                        0.27904000m,
                        0.27807000m,
                        0.27807000m,
                        238.87600000m,
                        1523386799999,
                        66.46759644m,
                        106,
                        186.54500000m,
                        51.90564846m,
                        0
                    },
                    {
                        1523386800000,
                        0.27874000m,
                        0.27880000m,
                        0.27760000m,
                        0.27825000m,
                        141.58400000m,
                        1523387699999,
                        39.37949124m,
                        169,
                        29.66700000m,
                        8.25734766m,
                        0
                    },
                    {
                        1523387700000,
                        0.27825000m,
                        0.27825000m,
                        0.27825000m,
                        0.27825000m,
                        0.00000000m,
                        1523388599999,
                        0.00000000m,
                        0,
                        0.00000000m,
                        0.00000000m,
                        0
                    }
                };

                httpTest.RespondWithJson(respArr);

                var res = await binanceService.GetKlines(new GetKlinesRequest { Symbol = "LTCETH", Interval = Constants.INTERVAL_15m });

                Assert.Equal(3, res.Count);

                int i = 0;
                foreach (var kline in res)
                {
                    Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds((long)respArr[i, 0]).LocalDateTime, kline.OpenTime);
                    Assert.Equal(respArr[i, 1], kline.Open);
                    Assert.Equal(respArr[i, 2], kline.High);
                    Assert.Equal(respArr[i, 3], kline.Low);
                    Assert.Equal(respArr[i, 4], kline.Close);
                    Assert.Equal(respArr[i, 5], kline.Volume);
                    Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds((long)respArr[i, 6]).LocalDateTime, kline.CloseTime);
                    Assert.Equal(respArr[i, 8], kline.NumTrades);

                    i++;
                }
            }
        }

        [Fact]
        public async void GetEmptyKline()
        {
            TestConfiguration config = new TestConfiguration()
            {
                Settings = new Dictionary<string, string>
                {
                    { "Binance:Url", "https://api.binance.com" }
                }
            };

            var binanceService = new BinanceApiService(config);

            using (var httpTest = new HttpTest())
            {
                object[,] respArr = new object[,]
                {
                    {

                    }
                };

                httpTest.RespondWithJson(respArr);

                var res = await binanceService.GetKlines(new GetKlinesRequest { Symbol = "LTCETH", Interval = Constants.INTERVAL_15m });

                Assert.Empty(res);
            }
        }

        [Fact]
        public async void GetAccountInformation()
        {
            TestConfiguration config = new TestConfiguration()
            {
                Settings = new Dictionary<string, string>
                {
                    { "Binance:Url", "https://api.binance.com" },
                    { "Binance:PublicApiKey", "hej"},
                    { "Binance:PrivateApiKey", "hopp"}
                }
            };

            var binanceService = new BinanceApiService(config);

            using (var httpTest = new HttpTest())
            {
                var respObj = new GetAccountInformationResponse
                {
                    MakerCommission = 15,
                    TakerCommission = 15,
                    BuyerCommission = 0,
                    SellerCommission = 0,
                    CanTrade = true,
                    CanWithdraw = true,
                    CanDeposit = true,
                    UpdateTime = new DateTime(2019, 1, 1, 12, 0, 0),
                    Balances = new List<Balance>
                    {
                        new Balance
                        {
                            Asset = "BTC",
                            Free = 15.15749m,
                            Locked = 0
                        },
                        new Balance
                        {
                            Asset = "LTC",
                            Free = 175.15749m,
                            Locked = 0.6334m
                        }
                    }
                };

                httpTest.RespondWithJson(respObj);

                var res = await binanceService.GetAccountInformation(new GetAccountInformationRequest { Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds() });

                Assert.Equal(JsonConvert.SerializeObject(respObj), JsonConvert.SerializeObject(res));
            }
        }

        [Fact]
        public async void GetAccountInformationWithEmptyBalancesRemoved()
        {
            TestConfiguration config = new TestConfiguration()
            {
                Settings = new Dictionary<string, string>
                {
                    { "Binance:Url", "https://api.binance.com" },
                    { "Binance:PublicApiKey", "hej"},
                    { "Binance:PrivateApiKey", "hopp"}
                }
            };

            var binanceService = new BinanceApiService(config);

            using (var httpTest = new HttpTest())
            {
                var respObj = new GetAccountInformationResponse
                {
                    MakerCommission = 15,
                    TakerCommission = 15,
                    BuyerCommission = 0,
                    SellerCommission = 0,
                    CanTrade = true,
                    CanWithdraw = true,
                    CanDeposit = true,
                    UpdateTime = new DateTime(2019, 1, 1, 12, 0, 0),
                    Balances = new List<Balance>
                    {
                        new Balance
                        {
                            Asset = "BTC",
                            Free = 15.15749m,
                            Locked = 0
                        },
                        new Balance
                        {
                            Asset = "LTC",
                            Free = 0,
                            Locked = 0.6334m
                        },
                        new Balance
                        {
                            Asset = "ETH",
                            Free = 0,
                            Locked = 0
                        }
                    }
                };

                httpTest.RespondWithJson(respObj);

                var res = await binanceService.GetAccountInformation(new GetAccountInformationRequest { Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds() });

                respObj.Balances.RemoveAll(x => x.Free == 0 && x.Locked == 0);

                Assert.Equal(JsonConvert.SerializeObject(respObj), JsonConvert.SerializeObject(res));
            }
        }

        [Fact]
        public async void GetBalance()
        {
            TestConfiguration config = new TestConfiguration()
            {
                Settings = new Dictionary<string, string>
                {
                    { "Binance:Url", "https://api.binance.com" },
                    { "Binance:PublicApiKey", "hej"},
                    { "Binance:PrivateApiKey", "hopp"}
                }
            };

            var binanceService = new BinanceApiService(config);

            using (var httpTest = new HttpTest())
            {
                var respObj = new GetAccountInformationResponse
                {
                    MakerCommission = 15,
                    TakerCommission = 15,
                    BuyerCommission = 0,
                    SellerCommission = 0,
                    CanTrade = true,
                    CanWithdraw = true,
                    CanDeposit = true,
                    UpdateTime = new DateTime(2019, 1, 1, 12, 0, 0),
                    Balances = new List<Balance>
                    {
                        new Balance
                        {
                            Asset = "BTC",
                            Free = 15.15749m,
                            Locked = 0
                        },
                        new Balance
                        {
                            Asset = "LTC",
                            Free = 0,
                            Locked = 0.6334m
                        }
                    }
                };

                httpTest.RespondWithJson(respObj);

                var res = await binanceService.GetBalance("BTC");

                Assert.Equal(respObj.Balances.Single(x => x.Asset == "BTC").Free, res);
            }
        }

        [Fact]
        public async void GetOrderBook()
        {
            TestConfiguration config = new TestConfiguration()
            {
                Settings = new Dictionary<string, string>
                {
                    { "Binance:Url", "https://api.binance.com" }
                }
            };

            var binanceService = new BinanceApiService(config);

            using (var httpTest = new HttpTest())
            {
                dynamic respObj = new
                {
                    lastUpdateId = 1027024,
                    bids = new string[,] {
                        {
                            "4.00000000",
                            "431.00000000"
                        },
                        {
                            "6.04654400",
                            "42133.5400"
                        }
                    },
                    asks = new string[,] {
                        {
                            "4.00000200",
                            "12.00000000"
                        },
                        {
                            "0.43332242",
                            "1.97434200"
                        }
                    },
                };

                httpTest.RespondWithJson(respObj);

                var res = await binanceService.GetOrderBook(new GetOrderBookRequest { Limit = 2, Symbol = "LTCETH" });

                Assert.Equal((int)respObj.lastUpdateId, res.LastUpdateId);

                Assert.NotNull(res.Bids.SingleOrDefault(x => x.Price == 4.00000000m && x.Quantity == 431.00000000m));
                Assert.NotNull(res.Bids.SingleOrDefault(x => x.Price == 6.04654400m && x.Quantity == 42133.5400m));
                Assert.NotNull(res.Asks.SingleOrDefault(x => x.Price == 4.00000200m && x.Quantity == 12.00000000m));
                Assert.NotNull(res.Asks.SingleOrDefault(x => x.Price == 0.43332242m && x.Quantity == 1.97434200m));
            }
        }

        [Fact]
        public async void MakeMarketTrade()
        {
            TestConfiguration config = new TestConfiguration()
            {
                Settings = new Dictionary<string, string>
                {
                    { "Binance:Url", "https://api.binance.com" },
                    { "Binance:PublicApiKey", "hej"},
                    { "Binance:PrivateApiKey", "hopp"}
                }
            };

            var binanceService = new BinanceApiService(config);

            using (var httpTest = new HttpTest())
            {
                var respBody = @"{
                                  'symbol': 'BTCUSDT',
                                  'orderId': 28,
                                  'clientOrderId': '6gCrw2kRUAF9CvJDGP16IP',
                                  'transactTime': 1507725176595,
                                  'price': '0.00000000',
                                  'origQty': '10.00000000',
                                  'executedQty': '10.00000000',
                                  'status': 'FILLED',
                                  'timeInForce': 'GTC',
                                  'type': 'MARKET',
                                  'side': 'SELL',
                                  'fills': [
                                    {
                                      'price': '4000.00000000',
                                      'qty': '1.00000000',
                                      'commission': '4.00000000',
                                      'commissionAsset': 'USDT'
                                    },
                                    {
                                      'price': '3999.00000000',
                                      'qty': '5.00000000',
                                      'commission': '19.99500000',
                                      'commissionAsset': 'USDT'
                                    },
                                    {
                                      'price': '3998.00000000',
                                      'qty': '2.00000000',
                                      'commission': '7.99600000',
                                      'commissionAsset': 'USDT'
                                    },
                                    {
                                      'price': '3997.00000000',
                                      'qty': '1.00000000',
                                      'commission': '3.99700000',
                                      'commissionAsset': 'USDT'
                                    },
                                    {
                                      'price': '3995.00000000',
                                      'qty': '1.00000000',
                                      'commission': '3.99500000',
                                      'commissionAsset': 'USDT'
                                    }
                                  ]
                                }";

                httpTest.RespondWith(respBody);

                var res = await binanceService.MakeTrade(new TradeRequest { });
                var respObj = JsonConvert.DeserializeObject<TradeResponse>(respBody);

                Assert.Equal(JsonConvert.SerializeObject(respObj), JsonConvert.SerializeObject(res), ignoreCase: true, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
            }
        }

        [Fact]
        public async void GetTickers()
        {
            TestConfiguration config = new TestConfiguration()
            {
                Settings = new Dictionary<string, string>
                {
                    { "Binance:Url", "https://api.binance.com" },
                    { "Binance:PublicApiKey", "hej"},
                    { "Binance:PrivateApiKey", "hopp"}
                }
            };

            var binanceService = new BinanceApiService(config);

            using (var httpTest = new HttpTest())
            {
                var respStr = @"
                                [
                                  {
                                    'symbol': 'LTCBTC',
                                    'price': '4.00000200'
                                  },
                                  {
                                    'symbol': 'ETHBTC',
                                    'price': '0.07946600'
                                  }
                                ]
                                ";

                httpTest.RespondWith(respStr);

                var res = await binanceService.GetAllTickers();

                Assert.Equal(2, res.Count);
                Assert.Contains(res, x => x.Symbol == "LTCBTC");
                Assert.Contains(res, x => x.Symbol == "ETHBTC");
                Assert.Equal(4.000002M, res.Single(x => x.Symbol == "LTCBTC").Price);
                Assert.Equal(0.07946600M, res.Single(x => x.Symbol == "ETHBTC").Price);
            }
        }

        [Fact(Skip = "SKIPIT")]
        public async void DoAnything()
        {
            TestConfiguration config = new TestConfiguration()
            {
                Settings = new Dictionary<string, string>
                {
                    { "Binance:Url", "https://api.binance.com" },
                    { "Binance:PublicApiKey", "FrdI9jByALplSESs0QwEIKxemRv6rlQ9SRobE7GMhT3X8clX7LHbRmKTYCBm96AD"},
                    { "Binance:PrivateApiKey", "7SocigMSdVJOpVyHizzBEWK3dFuLCDOFIKQqzmIYdy9Nb9L1Q7GGQ8V0jNhuQqJf"}
                }
            };

            var binanceService = new BinanceApiService(config);

            var req = new GetOrderBookRequest { Limit = 10, Symbol = "EOSETH" };
            //var req = new TradeRequest { Symbol = "LTCETH", Quantity = 0.099M, Side = Enums.OrderSides.SELL, Timestamp = Util.GetTimestamp(), Type = Enums.TradeType.LIMIT, TimeInForce = Enums.TimeInForce.FOK, Price = 0.194556m, NewOrderRespType = Enums.OrderResponseType.FULL };
            //var req = new GetAccountInformationRequest { Timestamp = Util.GetTimestamp() };

            var res = await binanceService.GetOrderBook(req);
            
            //var res = await binanceService.GetAccountInformation(req);
        }
    }

    public class TestConfiguration : IConfiguration
    {
        public Dictionary<string, string> Settings { get; set; }

        public string this[string key]
        {
            get
            {
                return Settings.ContainsKey(key) ? Settings[key] : null;
            }
            set =>
                throw new NotImplementedException();
        }

        public IEnumerable<IConfigurationProvider> Providers => throw new NotImplementedException();

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            throw new NotImplementedException();
        }

        public IChangeToken GetReloadToken()
        {
            throw new NotImplementedException();
        }

        public IConfigurationSection GetSection(string key)
        {
            throw new NotImplementedException();
        }

        public void Reload()
        {
            throw new NotImplementedException();
        }
    }
}
