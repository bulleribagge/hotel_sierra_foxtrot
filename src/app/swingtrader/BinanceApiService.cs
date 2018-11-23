using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using swingtrader.Domain;
using System;
using System.Collections.Generic;
using Flurl.Http;
using Flurl;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;
using Serilog;

namespace swingtrader
{
    public class BinanceApiService : ITradingApi
    {
        private readonly IConfiguration _configuration;

        public BinanceApiService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<Balance>> GetAllBalances()
        {
            var accountInformation = await GetAccountInformation(new GetAccountInformationRequest { Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds() });

            return accountInformation.Balances;
        }

        public async Task<decimal> GetBalance(string symbol)
        {
            var accountInformation = await GetAccountInformation(new GetAccountInformationRequest { Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds() });

            if (accountInformation != null)
            {
                var balance = accountInformation.Balances.SingleOrDefault(x => x.Asset.ToUpper() == symbol.ToUpper());

                if (balance != null)
                {
                    return balance.Free;
                }
            }
            return 0;
        }

        public async Task<GetAccountInformationResponse> GetAccountInformation(GetAccountInformationRequest req)
        {
            try
            {
                var res = await _configuration["Binance:Url"]
                    .AppendPathSegments("api", "v3", "account")
                    .SetQueryParams(new
                    {
                        timestamp = req.Timestamp
                    })
                    .WithHeader("X-MBX-APIKEY", _configuration["Binance:PublicApiKey"])
                    .AppendHmac(_configuration["Binance:PrivateApiKey"])
                    .GetJsonAsync<GetAccountInformationResponse>();

                res.Balances = res.Balances.Where(x => x.Free != 0 || x.Locked != 0).ToList();

                return res;
            }catch(FlurlHttpException ex)
            {
                Log.Error(ex, "Error trying to get account information");
            }

            return null;
        }

        public async Task<List<Kline>> GetKlines(GetKlinesRequest request)
        {
            List<Kline> klines = new List<Kline>();

            Object reqObj = null;

            if(request.Limit.HasValue)
            {
                reqObj = new
                {
                    symbol = request.Symbol,
                    interval = request.Interval,
                    limit = request.Limit.Value
                };
            }
            else
            {
                reqObj = new
                {
                    symbol = request.Symbol,
                    interval = request.Interval
                };
            }

            try
            {
                var resStr = await _configuration["Binance:Url"]
                        .AppendPathSegments("api", "v1", "klines")
                        .SetQueryParams(reqObj)
                        .GetStringAsync();

                var f = JArray.Parse(resStr);

                foreach (var item in f)
                {
                    if (item.HasValues)
                    {
                        klines.Add(new Kline(item, request.Symbol));
                    }
                }

                return klines;
            }
            catch (FlurlHttpException ex)
            {
                Log.Error(ex, $"Error trying to fetch klines. Response: {await ex.GetResponseStringAsync()}");
            }

            return null;
        }

        public async Task<TradeResponse> MakeTrade(TradeRequest req)
        {
            try
            {
                var res = await _configuration["Binance:Url"]
                    .AppendPathSegments("api", "v3", "order")
                    .SetQueryParams(new
                    {
                        symbol = req.Symbol,
                        side = req.Side.ToString(),
                        type = req.Type.ToString(),
                        quantity = req.Quantity,
                        timestamp = req.Timestamp,
                        newOrderRespType = req.NewOrderRespType.ToString()
                    })
                    .AppendHmac(_configuration["Binance:PrivateApiKey"])
                    .WithHeader("X-MBX-APIKEY", _configuration["Binance:PublicApiKey"])
                    .PostAsync(null)
                    .ReceiveJson<TradeResponse>();

                return res;
            }
            catch (FlurlHttpException ex)
            {
                Log.Error(ex, $"Error trying to place trade. Response: {await ex.GetResponseStringAsync()}");
            }

            return null;
        }

        public async Task<GetOrderBookResponse> GetOrderBook(GetOrderBookRequest request)
        {
            Object queryParams;

            if(request.Limit.HasValue)
            {
                queryParams = new {
                    symbol = request.Symbol,
                    limit = request.Limit.Value
                };
            }
            else
            {
                queryParams = new
                {
                    symbol = request.Symbol
                };
            }

            try
            {
                var resStr = await _configuration["Binance:Url"]
                        .AppendPathSegments("api", "v1", "depth")
                        .SetQueryParams(queryParams)
                        .GetStringAsync();

                var resObj = JsonConvert.DeserializeObject(resStr);

                return new GetOrderBookResponse(resObj);
            }
            catch (FlurlHttpException ex)
            {
                Log.Error(ex, $"Error when trying to get order book. Response: {ex.GetResponseStringAsync()}");
            }

            return null;
        }

        public async Task<List<Ticker>> GetAllTickers()
        {
            try
            {
                var res = await _configuration["Binance:Url"]
                    .AppendPathSegments("api", "v3", "ticker", "price")
                    .GetJsonAsync<List<Ticker>>();

                return res;
            }
            catch (FlurlHttpException ex)
            {
                Log.Error(ex, "Error trying to get tickers");
            }

            return null;
        }
    }
}

public interface ITradingApi
{
    Task<List<Kline>> GetKlines(GetKlinesRequest request);
    Task<TradeResponse> MakeTrade(TradeRequest request);
    Task<Decimal> GetBalance(string symbol);
    Task<List<Balance>> GetAllBalances();
    Task<List<Ticker>> GetAllTickers();
    Task<GetOrderBookResponse> GetOrderBook(GetOrderBookRequest symbol);
}
