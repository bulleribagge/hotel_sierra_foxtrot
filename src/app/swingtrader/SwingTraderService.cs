using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using swingtrader.Domain;
using System.Collections.Generic;
using System.Timers;
using System.Threading.Tasks;
using System;
using System.Linq;
using Serilog;
using Serilog.Context;

namespace swingtrader
{
    public class SwingTraderService : ITraderService
    {
        private readonly IConfiguration _configuration;
        private readonly IDatabaseService _databaseService;
        private readonly ITradingApi _tradingApi;
        private readonly TradingPairSettings _tradingPairSettings;
        private readonly ISymbolRateApi _symbolRateApi;
        private readonly ICurrencyRateApi _currencyRateApi;

        private Timer _processTimer;
        private Timer _symbolRateTimer;
        private Timer _currencyRateTimer;
        private Timer _portfolioBalanceTimer;

        private bool _testMode;

        public SwingTraderService(IConfiguration configuration, IDatabaseService databaseService, ITradingApi tradingApi, IOptions<TradingPairSettings> tradingPairSettings, ISymbolRateApi symbolRateApi, ICurrencyRateApi currencyRateApi)
        {
            _configuration = configuration;
            _databaseService = databaseService;
            _tradingApi = tradingApi;
            _tradingPairSettings = tradingPairSettings.Value;
            _symbolRateApi = symbolRateApi;
            _currencyRateApi = currencyRateApi;

            _testMode = bool.Parse(configuration["TradingSettings:TestMode"]);
        }

        public async Task Run()
        {
            Log.Debug("Running trader...");

            await _databaseService.Initialize();

            Parallel.ForEach(_tradingPairSettings.TradingPairs, (tradingPair) =>
            {
                tradingPair.CreatedAt = DateTime.Now;
                tradingPair.LastAction = Enums.OrderSides.SELL;
                tradingPair.BuyingEnabled = true;
                _databaseService.UpsertTradingPair(tradingPair);
            });

            foreach (var tradingPair in _tradingPairSettings.TradingPairs)
            {
                _symbolRateApi.AddSymbol(tradingPair.To);
            }

            InitializeTimers();

            StartTimers();

            await _currencyRateApi.RefreshRate();
            await _symbolRateApi.RefreshRates();
        }

        private void StartTimers()
        {
            _processTimer.Start();
            _symbolRateTimer.Start();
            _currencyRateTimer.Start();
            _portfolioBalanceTimer.Start();
        }

        private void InitializeTimers()
        {
            _processTimer = new Timer();
            _processTimer.Elapsed += new ElapsedEventHandler(Process);
            int processInterval;
            if (int.TryParse(_configuration["TimerIntervals:ProcessInterval"], out processInterval))
            {
                _processTimer.Interval = processInterval;
            }
            else
            {
                _processTimer.Interval = 60 * 1000;
            }

            _symbolRateTimer = new Timer();
            _symbolRateTimer.Elapsed += new ElapsedEventHandler(RefreshCryptoRates);
            int symbolRateRefreshInterval; 
            if (int.TryParse(_configuration["TimerIntervals:SymbolInterval"], out symbolRateRefreshInterval))
            {
                _symbolRateTimer.Interval = symbolRateRefreshInterval;
            }
            else
            {
                _symbolRateTimer.Interval = 60 * 1000;
            }

            _currencyRateTimer = new Timer();
            _currencyRateTimer.Elapsed += new ElapsedEventHandler(RefreshCurrencyRates);
            int currencyRateRefreshInterval;
            if (int.TryParse(_configuration["TimerIntervals:CurrencyInterval"], out currencyRateRefreshInterval))
            {
                _currencyRateTimer.Interval = currencyRateRefreshInterval;
            }
            else
            {
                _currencyRateTimer.Interval = 24 * 60 * 60 * 1000;
            }

            _portfolioBalanceTimer = new Timer();
            _portfolioBalanceTimer.Elapsed += new ElapsedEventHandler(LogPortfolioBalance);
            int portfolioBalanceInterval;
            if (int.TryParse(_configuration["TimerIntervals:PortfolioBalance"], out portfolioBalanceInterval))
            {
                _portfolioBalanceTimer.Interval = portfolioBalanceInterval;
            }
            else
            {
                _portfolioBalanceTimer.Interval = 30 * 60 * 1000;
            }
        }

        private void RefreshCryptoRates(object source, ElapsedEventArgs e)
        {
            _symbolRateApi.RefreshRates();
        }

        private void RefreshCurrencyRates(object source, ElapsedEventArgs e)
        {
            _currencyRateApi.RefreshRate();
        }

        private async void LogPortfolioBalance(object source, ElapsedEventArgs e)
        {
            var balanceTask = _tradingApi.GetAllBalances();
            var tickerTask = _tradingApi.GetAllTickers();

            await Task.WhenAll(balanceTask, tickerTask);

            var balances = await balanceTask;
            var tickers = await tickerTask;

            var eth = balances.Single(x => x.Asset == "ETH");
            var ethBalance = eth.Total;

            Log.Debug("Portfolio balance for {symbol} : {balance}", eth.Asset, eth.Total);

            balances.Remove(eth);

            foreach (var balance in balances)
            {
                //Log individual balances
                Log.Debug("Portfolio balance for {symbol} : {balance}", balance.Asset, balance.Total);

                ethBalance += tickers.Single(x => x.Symbol == balance.Asset + "ETH").Price * balance.Total;                
            }

            var ethRate = _symbolRateApi.GetRateForSymbol("ETH");
            var ethValue = ethRate * ethBalance;
            var kronaRate = _currencyRateApi.GetKronaRate();
            var kronaValue = ethValue * kronaRate;

            Log.Debug("Total portfolio balance: ETH: {totalEth}, USD: {totalUSD}, SEK: {totalSEK}, ETH rate: {ethRate}, Krona rate: {kronaRate}", ethBalance, ethValue, kronaValue, ethRate, kronaRate);
        }

        public async void Process(object source, ElapsedEventArgs e)
        {
            var processTasks = new List<Task>();

            // fetch all pairs
            var tradingPairs = await _databaseService.GetAllTradingPairs();
            var taskFactory = new TaskFactory();

            foreach (var tradingPair in tradingPairs)
            {
                processTasks.Add(Task.Run(async () =>
                {
                    var req = new GetKlinesRequest
                    {
                        Interval = tradingPair.Interval,
                        Limit = 22,
                        Symbol = tradingPair.Pair
                    };

                    var klines = await _tradingApi.GetKlines(req);
                    
                    if(klines == null)
                    {
                        return;
                    }

                    var last = klines.Last();

                    tradingPair.LastCheck = DateTime.Now;
                    tradingPair.LastRate = last.Close;

                    // Calculate available balances for pair
                    var balanceFrom = 0.1M;
                    var balanceTo = await _tradingApi.GetBalance(tradingPair.From);

                    if (!_testMode)
                    {
                        if (tradingPair.LastAction == Enums.OrderSides.SELL)
                        {
                            var nonInvestedBalance = await _tradingApi.GetBalance(tradingPair.To);
                            var investedBalance = tradingPairs.Where(x => x.To == tradingPair.To).Sum(x => (x.UnitsInvested * x.LastBuyPrice));
                            var totalBalance = (nonInvestedBalance + nonInvestedBalance) * 0.95M; //leave some just to make sure we don't overdo it
                            balanceFrom = totalBalance / tradingPairs.Where(x => x.To == tradingPair.To).Count();
                        }
                    }

                    // buy? sell?
                    var bollinger = Util.CalculateBollinger(klines, 21, double.Parse(_configuration["TradingSettings:BollingerBandDeviationMultiplier"]));
                    var availableOrders = await _tradingApi.GetOrderBook(new GetOrderBookRequest() { Symbol = tradingPair.Pair, Limit = 100 });

                    if(availableOrders == null)
                    {
                        return;
                    }

                    var bestBuyPrice = Util.GetBestPriceForQuantity(availableOrders.Asks, balanceFrom / availableOrders.Asks.Min(x => x.Price));
                    Log.Debug("Rate for {pair}: {rate}. Bollinger values: {low}, {mid}, {high}", tradingPair.Pair, bestBuyPrice, bollinger.LowBand, bollinger.MiddleBand, bollinger.UpperBand);

                    if (tradingPair.LastAction == Enums.OrderSides.SELL)
                    {
                        if(bestBuyPrice == 0)
                        {
                            Log.Debug($"Could not reach quantity {balanceFrom / availableOrders.Asks.Min(x => x.Price)} for pair {tradingPair.Pair} when buying");
                            return;
                        }

                        if (bestBuyPrice < bollinger.LowBand && tradingPair.BuyingEnabled)
                        {
                            var price = Util.RoundDownToPrecision(bestBuyPrice, tradingPair.PriceStepSize);
                            var quantity = Util.RoundDownToPrecision(balanceFrom / price, tradingPair.LotStepSize);

                            var tradeReq = new TradeRequest() {
                                NewOrderRespType = Enums.OrderResponseType.FULL,
                                Quantity = quantity,
                                Side = Enums.OrderSides.BUY,
                                Symbol = tradingPair.Pair,
                                Timestamp = Util.GetTimestamp(),
                                Type = Enums.TradeType.MARKET
                            };

                            TradeResponse res = null;
                            if (!_testMode)
                            {
                                res = await _tradingApi.MakeTrade(tradeReq);
                            }
                            else
                            {
                                res = new TradeResponse { Status = Enums.TradeStatus.FILLED, Fills = new List<Fill> { new Fill { Commission = quantity * 0.001M, Price = price, Quantity = quantity } } };
                            }

                            if (res.Status == Enums.TradeStatus.FILLED)
                            {
                                var fee = res.Fills.Sum(x => x.Commission);
                                tradingPair.LastAction = Enums.OrderSides.BUY;
                                tradingPair.LastBuyPrice = Util.GetAveragePriceOfFills(res.Fills);
                                tradingPair.UnitsInvested = quantity - fee;

                                Log.Information("Bought {quantity} {from} @ {price} {to} each for a total of {total}, with a fee of {fee}", quantity, tradingPair.From, tradingPair.LastBuyPrice, tradingPair.To, tradingPair.LastBuyPrice * quantity, fee);
                            }
                            else
                            {
                                Log.Information("Bought {pair} and got res: {res}", tradingPair.Pair, JsonConvert.SerializeObject(res));
                                // Do something clever
                            }
                        }
                    }else if (tradingPair.LastAction == Enums.OrderSides.BUY)
                    {
                        var bestSellPrice = Util.GetBestPriceForQuantity(availableOrders.Bids, balanceTo);
                        if (bestSellPrice == 0)
                        {
                            Log.Debug($"Could not reach quantity {tradingPair.UnitsInvested} for pair {tradingPair.Pair} when selling");
                            return;
                        }
                        var priceDiff = Util.GetPriceDiffPercent(tradingPair.LastBuyPrice, bestSellPrice);

                        if(bestSellPrice > bollinger.UpperBand && bestSellPrice > tradingPair.LastBuyPrice && priceDiff > decimal.Parse(_configuration["TradingSettings:PriceDiffThreshold"]))
                        {
                            var price = Util.RoundDownToPrecision(bestSellPrice, tradingPair.PriceStepSize);
                            var quantity = Util.RoundDownToPrecision(balanceTo, tradingPair.LotStepSize);

                            var tradeReq = new TradeRequest()
                            {
                                NewOrderRespType = Enums.OrderResponseType.FULL,
                                Quantity = quantity,
                                Side = Enums.OrderSides.SELL,
                                Symbol = tradingPair.Pair,
                                Timestamp = Util.GetTimestamp(),
                                Type = Enums.TradeType.MARKET
                            };

                            TradeResponse res = null;
                            if(!_testMode)
                            {
                                res = await _tradingApi.MakeTrade(tradeReq);
                            }
                            else
                            {
                                res = new TradeResponse { Status = Enums.TradeStatus.FILLED, Fills = new List<Fill> { new Fill { Commission = (quantity * price) * 0.001M, Price = price, Quantity = quantity } } };
                            }

                            if(res.Status == Enums.TradeStatus.FILLED)
                            {
                                var fee = res.Fills.Sum(x => x.Commission);
                                var soldFor = Util.GetAveragePriceOfFills(res.Fills);
                                var soldForTotal = soldFor * quantity;
                                var boughtForTotal = tradingPair.LastBuyPrice * quantity;
                                var profit = soldForTotal - fee - boughtForTotal;
                                var profitPercent = Util.GetPriceDiffPercent(soldForTotal - fee, boughtForTotal);
                                Log.Information("Sold {quantity} {from} @ {soldFor} {to} each for a total of {total}, with a fee of {fee}. Profit {profit}, profit percentage {profitPercentage}", quantity, tradingPair.From, soldFor, tradingPair.To, soldForTotal, fee, profit, profitPercent);
                                
                                tradingPair.LastAction = Enums.OrderSides.SELL;
                                tradingPair.LastBuyPrice = 0;
                                tradingPair.UnitsInvested = 0;
                                tradingPair.ProfitPercent += profitPercent;
                                tradingPair.Profit += profit;
                                tradingPair.NumTrades++;
                            }else
                            {
                                Log.Information("Sold {pair} and got res: {res}", tradingPair.Pair, JsonConvert.SerializeObject(res));
                            }
                        }
                    }

                    await _databaseService.UpsertTradingPair(tradingPair);
                }));
            }

            await Task.WhenAll(processTasks);
        }
    }

    public interface ITraderService
    {
        Task Run();
    }
}
