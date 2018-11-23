using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace swingtrader
{
    public class BitstampApiService : ISymbolRateApi
    {
        private readonly IConfiguration _configuration;
        private ConcurrentBag<Rate> _rates;

        public BitstampApiService(IConfiguration configuration)
        {
            _configuration = configuration;
            _rates = new ConcurrentBag<Rate>();
        }

        public void AddSymbol(string symbol)
        {
            if(!_rates.Any(x => x.Symbol.Equals(symbol, StringComparison.InvariantCultureIgnoreCase)))
            {
                _rates.Add(new Rate
                {
                    Symbol = symbol,
                    RateValue = 0
                });
            }
        }

        public decimal GetRateForSymbol(string symbol)
        {
            return _rates.SingleOrDefault(x => x.Symbol.Equals(symbol, StringComparison.InvariantCultureIgnoreCase))?.RateValue ?? 0;
        }

        private async Task<decimal> GetRateFromApi(string symbol)
        {
            try
            {
                var res = await _configuration["Bitstamp:Url"]
                .AppendPathSegments("api", "v2", "ticker", symbol.ToLower() + "usd")
                .GetJsonAsync<GetSymbolRateResponse>();
                return res.Last;
            }
            catch (FlurlHttpException ex)
            {
                Log.Error(ex, $"Error when trying to fetch symbol rates from Bitstamp: {await ex.GetResponseStringAsync()}");
                return GetRateForSymbol(symbol);
            }
        }

        public async Task RefreshRates()
        {
            foreach (var rate in _rates)
            {
                rate.RateValue = await GetRateFromApi(rate.Symbol);
            }
        }
    }

    public class GetSymbolRateResponse
    {
        public decimal Last { get; set; }
    }

    public class Rate
    {
        public string Symbol { get; set; }
        public decimal RateValue { get; set; }
    }

    public interface ISymbolRateApi
    {
        decimal GetRateForSymbol(string symbol);
        Task RefreshRates();
        void AddSymbol(string symbol);
    }
}
