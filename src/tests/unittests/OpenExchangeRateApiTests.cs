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
    public class OpenExchangeRateApiTests
    {
        [Fact]
        public async void ReloadAndGetRate()
        {
            TestConfiguration config = new TestConfiguration()
            {
                Settings = new Dictionary<string, string>
                {
                    { "OpenExchangeRate:Url", "https://openexchangerates.org/" }
                }
            };

            var api = new OpenexchangeRatesApiService(config);

            using (var httpTest = new HttpTest())
            {
                string respJson = @"{
                                        'disclaimer': 'Usage subject to terms: https://openexchangerates.org/terms',
                                        'license': 'https://openexchangerates.org/license',
                                        'timestamp': 1525863600,
                                        'base': 'USD',
                                        'rates': {
                                            'SEK': 8.742603
                                        }
                                    }";

                httpTest.RespondWith(respJson);

                await api.RefreshRate();
                var res = api.GetKronaRate();
                Assert.Equal(8.742603m, res);
            }
        }

        [Fact]
        public async void ReloadAndGetRateWhenApiIsDownAndThenUpAgain()
        {
            TestConfiguration config = new TestConfiguration()
            {
                Settings = new Dictionary<string, string>
                {
                    { "OpenExchangeRate:Url", "https://openexchangerates.org/" }
                }
            };

            var api = new OpenexchangeRatesApiService(config);

            using (var httpTest = new HttpTest())
            {
                string respJson = @"{
                                        'disclaimer': 'Usage subject to terms: https://openexchangerates.org/terms',
                                        'license': 'https://openexchangerates.org/license',
                                        'timestamp': 1525863600,
                                        'base': 'USD',
                                        'rates': {
                                            'SEK': 8.742603
                                        }
                                    }";

                httpTest.RespondWith("error", status: 500)
                    .RespondWith(respJson);

                await api.RefreshRate();
                var res = api.GetKronaRate();
                Assert.Equal(0m, res);

                await api.RefreshRate();
                res = api.GetKronaRate();
                Assert.Equal(8.742603m, res);
            }
        }

        [Fact]
        public async void ReloadAndGetOldRateWhenApiIsUpAndThenDown()
        {
            TestConfiguration config = new TestConfiguration()
            {
                Settings = new Dictionary<string, string>
                {
                    { "OpenExchangeRate:Url", "https://openexchangerates.org/" }
                }
            };

            var api = new OpenexchangeRatesApiService(config);

            using (var httpTest = new HttpTest())
            {
                string respJson = @"{
                                        'disclaimer': 'Usage subject to terms: https://openexchangerates.org/terms',
                                        'license': 'https://openexchangerates.org/license',
                                        'timestamp': 1525863600,
                                        'base': 'USD',
                                        'rates': {
                                            'SEK': 8.742603
                                        }
                                    }";

                httpTest.RespondWith(respJson)
                    .RespondWith("error", status: 500);

                await api.RefreshRate();
                var res = api.GetKronaRate();
                Assert.Equal(8.742603m, res);

                await api.RefreshRate();
                res = api.GetKronaRate();
                Assert.Equal(8.742603m, res);

            }
        }
    }
}
