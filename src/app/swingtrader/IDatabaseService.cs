using swingtrader.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace swingtrader
{
    public interface IDatabaseService
    {
        Task Initialize();
        Task UpsertTradingPair(TradingPair tradingPair);
        Task<TradingPair> GetTradingPair(string from, string to, string interval);
        Task<List<TradingPair>> GetAllTradingPairs();
    }
}
