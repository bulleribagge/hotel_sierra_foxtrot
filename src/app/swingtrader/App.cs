using System;
using System.Threading.Tasks;

namespace swingtrader
{
    public class App
    {
        private readonly ITraderService _traderService;
        
        public App(ITraderService tradingService)
        {
            _traderService = tradingService;
        }

        public async Task Run()
        {
            await _traderService.Run();
        }
    }
}
