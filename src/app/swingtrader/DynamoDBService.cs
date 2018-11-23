using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using swingtrader.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace swingtrader
{
    public class DynamoDBService : IDatabaseService
    {
        private AmazonDynamoDBClient _client;
        private IConfiguration _config;

        public DynamoDBService(IConfiguration config)
        {
            _config = config;
        }

        public async Task UpsertTradingPair(TradingPair tradingPair)
        {
            var context = new DynamoDBContext(_client);
            await context.SaveAsync(tradingPair);
        }

        public async Task<TradingPair> GetTradingPair(string from, string to, string interval)
        {
            var context = new DynamoDBContext(_client);
            var res = await context.LoadAsync<TradingPair>(from + to + interval);
            return res;
        }

        public async Task<List<TradingPair>> GetAllTradingPairs()
        {
            List<TradingPair> tradingPairs = new List<TradingPair>();

            var context = new DynamoDBContext(_client);
            var res = context.ScanAsync<TradingPair>(null);
            while (!res.IsDone)
            {
                tradingPairs.AddRange(await res.GetNextSetAsync());
            }

            return tradingPairs;
        }

        public async Task Initialize()
        {
            var config = new AmazonDynamoDBConfig();
            config.ServiceURL = _config["DynamoDB:Url"];
            var credentials = new BasicAWSCredentials(_config["DynamoDB:Username"], _config["DynamoDB:Password"]);
            _client = new AmazonDynamoDBClient(credentials, config);

            if (_config["RecreateTables"] != null && bool.Parse(_config["RecreateTables"]))
            {
                await DeleteTables();
            }

            await CreateTables();
        }

        private async Task DeleteTables()
        {
            List<string> tables = new List<string>
            {
                "TradingPair"
            };

            List<Task> tableDeletes = new List<Task>();

            foreach (var table in tables)
            {
                tableDeletes.Add(Task.Run(() => DeleteTable(table)));
            }

            Task.WaitAll(tableDeletes.ToArray());
        }

        private async Task DeleteTable(string tableName)
        {
            var req = new DeleteTableRequest
            {
                TableName = tableName
            };

            await _client.DeleteTableAsync(req);
        }

        private async Task CreateTables()
        {
            var req = new CreateTableRequest
            {
                TableName = "TradingPair",
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        AttributeName = "Id",
                        AttributeType = "S"
                    }
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = "Id",
                        KeyType = "HASH"
                    }
                },
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 10,
                    WriteCapacityUnits = 10
                }
            };

            try
            {
                var res = await _client.CreateTableAsync(req);
            }
            catch (ResourceInUseException)
            {
            }
        }
    }
}
