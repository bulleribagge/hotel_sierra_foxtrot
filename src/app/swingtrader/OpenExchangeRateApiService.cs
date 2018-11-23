using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace swingtrader
{
    public class OpenexchangeRatesApiService : ICurrencyRateApi
    {
        private readonly IConfiguration _configuration;
        private decimal _kronaRate;

        public decimal KronaRate { get => _kronaRate; set => _kronaRate = value; }

        public OpenexchangeRatesApiService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task RefreshRate()
        {
            try
            {
                var res = await _configuration["OpenExchangeRate:Url"]
                .AppendPathSegments("api", "latest.json")
                .SetQueryParams(new
                {
                    app_id = _configuration["OpenExchangeRate:AppId"],
                    @base = "USD",
                    symbols = "SEK"
                })
                .GetJsonAsync();

                KronaRate = (decimal)res.rates.SEK;
            }
            catch(FlurlHttpException ex)
            {
                Log.Error(ex, $"Error when trying to fetch currency rate from OpenExchangeRateApi: {await ex.GetResponseStringAsync()}");
            }
        }

        public decimal GetKronaRate()
        {
            return KronaRate;
        }
    }

    public interface ICurrencyRateApi
    {
        Task RefreshRate();
        decimal GetKronaRate();
    }
}
