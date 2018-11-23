using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using swingtrader;
using swingtrader.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace unittests
{
    public class IntegrationTests
    {
        [Fact]
        public async void SaveAndGetTradingPair()
        {
            TestConfiguration config = new TestConfiguration()
            {
                Settings = new Dictionary<string, string>
                {
                    { "DynamoDB:Url", "http://dynamodb:8000" },
                    { "DynamoDB:Password", " "},
                    { "DynamoDB:Username", " "},
                    { "RecreateTables", "false"}
                }
            };

            DynamoDBService db = new DynamoDBService(config);

            var now = DateTime.Now;

            TradingPair tradingPair = new TradingPair
            {
                CreatedAt = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Kind),
                From = "VEN",
                To = "ETH",
                Interval = "15m",
                LastAction = Enums.OrderSides.SELL,
                LastBuyPrice = 0,
                LastCheck = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Kind),
                LastRate = 0,
                NumTrades = 0,
                ProfitPercent = 0
            };

            await db.Initialize();

            await db.UpsertTradingPair(tradingPair);

            var res = await db.GetTradingPair(tradingPair.From, tradingPair.To, tradingPair.Interval);

            Assert.NotNull(res);

            Assert.Equal(JsonConvert.SerializeObject(res), JsonConvert.SerializeObject(tradingPair));
        }

        [Fact]
        public async void SaveAndGet2TradingPairs()
        {
            TestConfiguration config = new TestConfiguration()
            {
                Settings = new Dictionary<string, string>
                {
                    { "DynamoDB:Url", "http://dynamodb:8000" },
                    { "DynamoDB:Password", " "},
                    { "DynamoDB:Username", " "},
                    { "RecreateTables", "false"}
                }
            };

            DynamoDBService db = new DynamoDBService(config);

            var now = DateTime.Now;

            TradingPair tradingPair = new TradingPair
            {
                CreatedAt = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Kind),
                From = "VEN",
                To = "ETH",
                Interval = "15m",
                LastAction = Enums.OrderSides.SELL,
                LastBuyPrice = 0,
                LastCheck = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Kind),
                LastRate = 0,
                NumTrades = 0,
                ProfitPercent = 0
            };

            await db.Initialize();

            await db.UpsertTradingPair(tradingPair);

            tradingPair.Interval = "30m";

            await db.UpsertTradingPair(tradingPair);

            var res = await db.GetTradingPair(tradingPair.From, tradingPair.To, tradingPair.Interval);

            Assert.NotNull(res);

            Assert.Equal(JsonConvert.SerializeObject(res), JsonConvert.SerializeObject(tradingPair));
        }

        [Fact]
        public async void SaveAndGetAllTradingPairs()
        {
            TestConfiguration config = new TestConfiguration()
            {
                Settings = new Dictionary<string, string>
                {
                    { "DynamoDB:Url", "http://dynamodb:8000" },
                    { "DynamoDB:Password", " "},
                    { "DynamoDB:Username", " "},
                    { "RecreateTables", "false"}
                }
            };

            DynamoDBService db = new DynamoDBService(config);

            var now = DateTime.Now;

            TradingPair tradingPair = new TradingPair
            {
                CreatedAt = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Kind),
                From = "VEN",
                To = "ETH",
                Interval = "15m",
                LastAction = Enums.OrderSides.SELL,
                LastBuyPrice = 0,
                LastCheck = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Kind),
                LastRate = 0,
                NumTrades = 0,
                ProfitPercent = 0
            };

            await db.Initialize();

            await db.UpsertTradingPair(tradingPair);

            tradingPair.Interval = "30m";

            await db.UpsertTradingPair(tradingPair);

            var res = await db.GetAllTradingPairs();

            Assert.NotEmpty(res);
            Assert.Equal(2, res.Count);

            Assert.Equal(JsonConvert.SerializeObject(res[1]), JsonConvert.SerializeObject(tradingPair));
            tradingPair.Interval = "15m";
            Assert.Equal(JsonConvert.SerializeObject(res[0]), JsonConvert.SerializeObject(tradingPair));
        }

        [Fact]
        public async void SaveUpdateAndGetTradingPair()
        {
            TestConfiguration config = new TestConfiguration()
            {
                Settings = new Dictionary<string, string>
                {
                    { "DynamoDB:Url", "http://dynamodb:8000" },
                    { "DynamoDB:Password", " "},
                    { "DynamoDB:Username", " "},
                    { "RecreateTables", "false"}
                }
            };

            DynamoDBService db = new DynamoDBService(config);

            var now = DateTime.Now;

            TradingPair tradingPair = new TradingPair
            {
                CreatedAt = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Kind),
                From = "VEN",
                To = "ETH",
                Interval = "15m",
                LastAction = Enums.OrderSides.SELL,
                LastBuyPrice = 0,
                LastCheck = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Kind),
                LastRate = 0,
                NumTrades = 0,
                ProfitPercent = 0
            };

            await db.Initialize();

            await db.UpsertTradingPair(tradingPair);

            tradingPair.LastRate = 0.003m;

            await db.UpsertTradingPair(tradingPair);

            var res = await db.GetTradingPair(tradingPair.From, tradingPair.To, tradingPair.Interval);

            Assert.NotNull(res);

            Assert.Equal(JsonConvert.SerializeObject(res), JsonConvert.SerializeObject(tradingPair));
        }
    }
}
