using Microsoft.VisualStudio.TestTools.UnitTesting;
using Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Tests
{
    [TestClass()]
    public class strat : IStrategy
    {
        public void AdjustStop(MarketData.Candle newCandle)
        {
            throw new NotImplementedException();
        }

        public OrderCondition GetCloseAllCondition(StrategyState s)
        {
            throw new NotImplementedException();
        }

        public DateTime GetCloseAllTime(StrategyState s)
        {
            throw new NotImplementedException();
        }

        public OrderCondition GetEntryCondition(StrategyState s)
        {
            throw new NotImplementedException();
        }

        public OrderCondition GetLimitConditions(StrategyState s)
        {
            throw new NotImplementedException();
        }

        public DateTime GetOpenTime(StrategyState s)
        {
            throw new NotImplementedException();
        }

        public OrderCondition GetStopConditions(StrategyState s)
        {
            throw new NotImplementedException();
        }
    }
    public class StrategyRunnerTests
    {
        StrategyRunner runner = new StrategyRunner();
        [TestMethod()]
        public void AddStrategyTest()
        {
            runner.AddStrategy(new strat(),new string[]{"AAPL"});

        }

        [TestMethod()]
        public void RunTest()
        {
            strat str = new strat();
             
        }
    }
}