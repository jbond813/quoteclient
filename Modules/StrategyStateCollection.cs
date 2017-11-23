using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules
{
    class StrategyStateCollection
    {
        public Dictionary<string, StrategyState> StrategyStates;
        public IStrategy Strategy;
        public StrategyStateCollection(IStrategy strat,string[] candidateSymbols)
        {
            StrategyStates = new Dictionary<string, StrategyState>();
            foreach(string can in candidateSymbols)
            {
                StrategyStates[can] = new StrategyState(strat);
            }
        }
    }
}
