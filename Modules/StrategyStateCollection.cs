using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules
{
    public class StrategyStateCollection
    {
        public Dictionary<string, StrategyState> StrategyStates;
        public IStrategy Strategy;
        public bool hasEvaluatedEntry = false;
        public StrategyStateCollection(IStrategy strat,Stock[] stocks)
        {
            Strategy = strat;
            StrategyStates = new Dictionary<string, StrategyState>();
            foreach(Stock stock in stocks)
            {
                StrategyStates[stock.Symbol] = new StrategyState(strat, stock);
            }
        }
    }
}
