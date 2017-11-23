using MarketData;
using MDPackets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules
{
    public interface IStrategy
    {
        DateTime GetOpenTime(StrategyState s);
        DateTime GetCloseAllTime(StrategyState s);
        OrderCondition GetEntryCondition(StrategyState s);
        OrderCondition GetLimitConditions(StrategyState s);
        OrderCondition GetStopConditions(StrategyState s);
        void AdjustStop(Candle newCandle);
        OrderCondition GetCloseAllCondition(StrategyState s);
    }

}
