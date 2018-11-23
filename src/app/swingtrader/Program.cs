using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using swingtrader.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;

namespace swingtrader
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();

            ConfigureServices(services);

            var serviceProvider = services.BuildServiceProvider();

            Task f = serviceProvider.GetService<App>().Run();

            f.GetAwaiter().GetResult();

            var ended = new ManualResetEventSlim();
            var starting = new ManualResetEventSlim();

            AssemblyLoadContext.Default.Unloading += ctx =>
            {
                Console.WriteLine("Unloading fired");
                starting.Set();
                Console.WriteLine("Waiting for completion");
                ended.Wait();
            };

            starting.Wait();

            Console.WriteLine("Received signal gracefully shutting down");
            Thread.Sleep(5000);
            ended.Set();
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(new LoggerFactory()
                .AddSerilog());

            serviceCollection.AddLogging();

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            try
            {
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .CreateLogger();
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            serviceCollection.AddSingleton(configuration);
            serviceCollection.AddTransient<ITraderService, SwingTraderService>();
            serviceCollection.AddTransient<IDatabaseService, DynamoDBService>();
            serviceCollection.AddTransient<ITradingApi, BinanceApiService>();
            serviceCollection.AddTransient<ISymbolRateApi, BitstampApiService>();
            serviceCollection.AddTransient<ICurrencyRateApi, OpenexchangeRatesApiService>();
            serviceCollection.Configure<TradingPairSettings>(configuration.GetSection("TradingPairSettings"));
            serviceCollection.AddTransient<App>();
        }
    }
}
