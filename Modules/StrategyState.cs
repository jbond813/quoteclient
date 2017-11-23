using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules
{
    public class StrategyState
    {
        public IStrategy strategy;
        public Order OpenOrder = null;
        public Order LimitOrder = null;
        public Order StopOrder = null;
        public int Position;
        public Stock Stock;
        public StrategyState(IStrategy strat)
        {
            strategy = strat;
        }
    }
}
