using Microsoft.VisualStudio.TestTools.UnitTesting;
using Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketData;
using System.Collections.Concurrent;
using MDPackets;
using TradingInterfaces;
using QuoteClientTester;

namespace Modules.Tests
{
    public class TestTimeProvider : ITimeProvider
    {
        public DateTime TimeToReturn;
        public DateTime GetCurrentTime()
        {
            return TimeToReturn;
        }
    }
    class strat : IStrategy
    {
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
            OrderCondition oc = new OrderCondition();
            //oc.Limit = s.Stock.Bid + .03;
            oc.Shares = 100;
            oc.Tif = OrderCondition.TIF.OPG;
            oc.Side = 'B';
            return oc;
        }

        public OrderCondition GetLimitConditions(StrategyState s)
        {
            return new OrderCondition() { Limit = 123.4, Shares = 100, Side = 'S', Tif = OrderCondition.TIF.DAY };
        }

        public DateTime GetOpenTime(StrategyState s)
        {
            return DateTime.Now.Date.AddHours(8.5);
        }
        public static ITimeProvider ttp;
        public OrderCondition GetStopConditions(StrategyState s)
        {
            OrderCondition rv = null;
            if (ttp.GetCurrentTime() > ttp.GetCurrentTime().Date + new TimeSpan(8, 45, 0))
            {
                Candle candle = s.Stock.CandleManager.Get(1);
                if(candle != null && s.Stock.LastTrade.Price < candle.Low - (candle.Low * .0025))
                {
                    rv = new OrderCondition()
                    {
                        Shares = s.Position,
                        Limit = s.Stock.LastTrade.Price - .25,
                        Side = 'S',
                        Tif = OrderCondition.TIF.DAY
                    };                     
                }
            }
            return rv;
        }
    }
    class TestExecutionServer : IExecutionServer
    {
        public static int orderID = 1;
        Dictionary<int, Order> orderDict = new Dictionary<int, Order>();
        public bool ExecuteOrder(int myOrderID, int shares)
        {
            bool rv = false;
            if (orderDict.ContainsKey(myOrderID))
            {
                rv = true;
                Order o = orderDict[myOrderID];
                queue.Add(new OrderExecuted($"TM_NEW_EXECUTION|{o.Symbol}|{o.Condition.Limit}|{shares}|{shares}|{o.Condition.Side}|{o.MyID + 1000}".Split('|')));
            }
            return rv;
        }
        public bool SendOrderMessage(int myOrderID)
        {
            bool rv = false;
            if (orderDict.ContainsKey(myOrderID))
            {
                rv = true;
                Order o = orderDict[myOrderID];
                queue.Add(new OrderSent($"ORDERSENT|{o.Symbol}|{o.MyID}|{o.MyID + 1000}".Split('|')));
                //queue.Add(new OrderExecuted($"TM_NEW_EXECUTION|{o.Symbol}|{o.Condition.Limit}|{shares}|{shares}|{o.Condition.Side}|{o.MyID + 1000}".Split('|')));
            }
            return rv;
        }
        public bool DeadifyOrder(int myOrderID, int shares)
        {
            bool rv = false;
            if (orderDict.ContainsKey(myOrderID))
            {
                rv = true;
                Order o = orderDict[myOrderID];
                queue.Add(new OrderDead($"ORDERDEAD|{o.Symbol}|{o.MyID  + 1000}|{o.Condition.Side}|{o.Condition.Shares}|{0}".Split('|')));
                //queue.Add(new OrderExecuted($"TM_NEW_EXECUTION|{o.Symbol}|{o.Condition.Limit}|{shares}|{shares}|{o.Condition.Side}|{o.MyID + 1000}".Split('|')));
            }
            return rv;
        }
        //public int SendOrder(char side, string symbol, double LimitPrice, double Bid, double Ask, string TIF, int qty)
        //{
        //    //symbol = toks[1];
        //    //Price = double.Parse(toks[2]);
        //    //ExecutionQuantity = Int32.Parse(toks[3]);
        //    //PositionQuantity = Int32.Parse(toks[4]);
        //    //Side = toks[5][0];
        //    //ClientOrderID = Int32.Parse(toks[6]);
        //    Order o = new Order() { Symbol = symbol, Condition = new OrderCondition() { Shares = qty, Limit = Bid, Side = side }, MyID = ReturnValue };
        //    orderDict.Add(ReturnValue, o);
        //    //queue.Add(new OrderSent($"ORDERSENT|{symbol}|{ReturnValue}|{ReturnValue + 1000}".Split('|')));
        //    //queue.Add(new OrderExecuted($"TM_NEW_EXECUTION|{symbol}|{Bid}|{qty}|{qty}|{side}|{ReturnValue + 1000}".Split('|')));
        //    //queue.Add(new OrderDead($"ORDERDEAD|{symbol}|{ReturnValue + 1000}|{side}|{qty}|{0}".Split('|')));

        //    return ReturnValue++;
        //}

        public Order SendOrder(OrderCondition cond, Stock s)
        {
            Order rv = new Order() { Condition = cond, MyID = orderID, LimitPrice = cond.Limit, Symbol = s.Symbol };
            orderDict[orderID++] = rv;
            return rv;
        }

        public void CancelOrder(Order o)
        {
            o.CancelSent = true;
        }

        public TestExecutionServer(BlockingCollection<Packet> queue)
        {
            this.queue = queue;
        }
        BlockingCollection<Packet> queue;
    }
    class TestQuoteServer : IQuoteServer
    {
        public void Subscribe(string Symbol)
        {
            return;
        }
    }
    [TestClass()]
    public class StrategyRunnerTests
    {
        class TestSetup
        {
            public TestTimeProvider ttp = new TestTimeProvider();
            public TestExecutionServer tex;
            public TestQuoteServer tqs = new TestQuoteServer();
            public BlockingCollection<Packet> queue = new BlockingCollection<Packet>();
            public StrategyState state;
            public StrategyRunner runner;
            public TestSetup(IStrategy strat,string[] symbols)
            {
                tex = new TestExecutionServer(queue);
                runner = new StrategyRunner(tex, tqs, ttp, queue);
                Modules.Tests.strat.ttp = ttp;
                runner.AddStrategy(strat, symbols);
                state = runner.GetStrategyStateCollections()[0].StrategyStates[symbols[0]];
            }
        }
        [TestMethod()]
        public void AddStrategyTest()
        {
            //run.AddStrategy(new strat(), new string[] { });
        }
        [TestMethod()]
        public void RunTest()
        {
            BlockingCollection<Packet> queue = new BlockingCollection<Packet>();
            TestExecutionServer tex = new TestExecutionServer(queue);
            TestQuoteServer tqs = new TestQuoteServer();
            TestTimeProvider ttp = new TestTimeProvider();
            StrategyRunner runner = new StrategyRunner(tex, tqs, ttp, queue);
            strat str = new Tests.strat();
            runner.AddStrategy(str, new string[] { "AAPL" });
            //run.queue.Add(MDPackets.Tests.TradeReportPacketTests.getPacket());
            StrategyStateCollection[] stratColls = runner.GetStrategyStateCollections();
            Assert.AreEqual(stratColls.Count(), 1);
            StrategyState state = stratColls[0].StrategyStates["AAPL"];
            state.Stock = new Stock("AAPL",ttp);
            new PrivateObject(state.Stock).SetField("bid",100);
            Assert.AreEqual(state.OpenOrder, null);
            ttp.TimeToReturn = DateTime.Now.Date.AddHours(8.51);
            PrivateObject po = new PrivateObject(runner);
            TestExecutionServer.orderID = 123;
            po.Invoke("RunOne");
            Assert.AreNotEqual(state.OpenOrder, null);
            Assert.AreEqual(state.OpenOrder.MyID, 123);
        }
        [TestMethod()]
        public void SentOrderTest()
        {
            BlockingCollection<Packet> queue = new BlockingCollection<Packet>();
            TestExecutionServer tex = new TestExecutionServer(queue);
            TestQuoteServer tqs = new TestQuoteServer();
            TestTimeProvider ttp = new TestTimeProvider();
            StrategyRunner runner = new StrategyRunner(tex, tqs, ttp, queue);
            strat str = new Tests.strat();
            runner.AddStrategy(str, new string[] { "AAPL" });
            //run.queue.Add(MDPackets.Tests.TradeReportPacketTests.getPacket());
            StrategyStateCollection[] stratColls = runner.GetStrategyStateCollections();
            Assert.AreEqual(stratColls.Count(), 1);
            StrategyState state = stratColls[0].StrategyStates["AAPL"];
            state.Stock = new Stock("AAPL",ttp);
            new PrivateObject(state.Stock).SetField("bid", 100);
            ttp.TimeToReturn = DateTime.Now.Date.AddHours(8.49);
            state.OpenOrder = new Order() { MyID = 42 };
            runner.queue.Add(new OrderSent("SENTORDER|AAPL|42|100042".Split('|')));
            PrivateObject po = new PrivateObject(runner);
            po.Invoke("RunOne");
            //Assert.AreNotEqual(state.OpenOrder, null);
            Assert.AreEqual(state.OpenOrder.TheirID, 100042);
        }
        [TestMethod()]
        public void OrderExecutedTest()
        {
            BlockingCollection<Packet> queue = new BlockingCollection<Packet>();
            TestExecutionServer tex = new TestExecutionServer(queue);
            TestQuoteServer tqs = new TestQuoteServer();
            TestTimeProvider ttp = new TestTimeProvider();
            StrategyRunner runner = new StrategyRunner(tex, tqs, ttp, queue);
            strat str = new Tests.strat();
            runner.AddStrategy(str, new string[] { "AAPL" });
            //run.queue.Add(MDPackets.Tests.TradeReportPacketTests.getPacket());
            StrategyStateCollection[] stratColls = runner.GetStrategyStateCollections();
            //Assert.AreEqual(stratColls.Count(), 1);
            StrategyState state = stratColls[0].StrategyStates["AAPL"];
            state.Stock = new Stock("AAPL",ttp);
            new PrivateObject(state.Stock).SetField("bid", 100);
            ttp.TimeToReturn = DateTime.Now.Date.AddHours(8.49);
            state.OpenOrder = new Order() { MyID = 42,TheirID = 100042 };
            //packetType = PacketType.ORDER_EXECUTED;
            //symbol = toks[1];
            //Price = double.Parse(toks[2]);
            //ExecutionQuantity = Int32.Parse(toks[3]);
            //PositionQuantity = Int32.Parse(toks[4]);
            //Side = toks[5][0];
            //ClientOrderID = Int32.Parse(toks[6]);
            runner.queue.Add(new OrderExecuted("TM_NEW_EXECUTION|AAPL|123.45|100|100|B|100042".Split('|')));
            //Assert.Fail("A");
            new PrivateObject(runner).Invoke("RunOne");
            Assert.AreEqual(100,state.Position);
        }
        [TestMethod()]
        public void SendOrderTest()
        {
            TestSetup ts = new TestSetup(new Tests.strat(), new string[] { "AAPL" });
            Assert.AreNotEqual(null, ts.state);
            OpenPosition(ts);
            Assert.AreEqual(100, ts.state.Position);
            //Assert.AreNotEqual(null, ts.state.LimitOrder);
        }
        void OpenPosition(TestSetup ts)
        {
            ts.ttp.TimeToReturn = DateTime.Now.Date.AddHours(8.5001);
            do
            {
                new PrivateObject(ts.runner).Invoke("RunOne");
            } while (ts.queue.Count != 0);
            Assert.AreNotEqual(null, ts.state.OpenOrder);
            Assert.AreNotEqual(0, ts.state.OpenOrder.MyID,"My ID not Set");
            ts.tex.SendOrderMessage(ts.state.OpenOrder.MyID);
            new PrivateObject(ts.runner).Invoke("RunOne");
            Assert.AreNotEqual(0, ts.state.OpenOrder.TheirID,"Their ID not Set");
            Assert.AreEqual(0, ts.state.Position);
            ts.tex.ExecuteOrder(ts.state.OpenOrder.MyID,100);
            new PrivateObject(ts.runner).Invoke("RunOne");
            Assert.AreEqual(100, ts.state.Position);
            Assert.AreEqual(null, ts.state.LimitOrder);
            ts.tex.DeadifyOrder(ts.state.OpenOrder.MyID, 100);
            new PrivateObject(ts.runner).Invoke("RunOne");
            Assert.AreNotEqual(null, ts.state.LimitOrder,"Limit Order Not Created");
            Assert.AreNotEqual(0, ts.state.LimitOrder.MyID, "Limit Order My ID not set");
            ts.tex.SendOrderMessage(ts.state.LimitOrder.MyID);
            new PrivateObject(ts.runner).Invoke("RunOne");
            Assert.AreNotEqual(0, ts.state.LimitOrder.TheirID, "Limit Order Their ID not set");
            DateTime ot = ts.ttp.GetCurrentTime().AddSeconds(1);
            ts.queue.Add(BuildFakeData.BuildFakeTradeReportPacket("AAPL", ts.ttp.GetCurrentTime().AddSeconds(1),1234,123.45,100, ts.ttp.GetCurrentTime().AddSeconds(1)));
            new PrivateObject(ts.runner).Invoke("RunOne");
            Assert.AreEqual(123.45, ts.state.Stock.CandleManager.Get(0).Close);

            ts.queue.Add(BuildFakeData.BuildFakeTradeReportPacket("AAPL", ts.ttp.GetCurrentTime().AddSeconds(2), 1234, 123.40, 100, ts.ttp.GetCurrentTime().AddSeconds(2)));
            ts.ttp.TimeToReturn = ts.ttp.GetCurrentTime().AddSeconds(5);
            new PrivateObject(ts.runner).Invoke("RunOne");
            Assert.AreEqual(123.40, ts.state.Stock.CandleManager.Get(0).Close,"Close");
            Assert.AreEqual(123.45, ts.state.Stock.CandleManager.Get(0).Open,"Open");
            Assert.AreEqual(123.45, ts.state.Stock.CandleManager.Get(0).High,"High");
            Assert.AreEqual(123.40, ts.state.Stock.CandleManager.Get(0).Low,"Low");
            ts.ttp.TimeToReturn = ts.ttp.GetCurrentTime().Date.AddHours(8.751);
            ts.queue.Add(BuildFakeData.BuildFakeTradeReportPacket("AAPL", ts.ttp.GetCurrentTime(), 1234, 110.40, 100, ts.ttp.GetCurrentTime()));
            new PrivateObject(ts.runner).Invoke("RunOne");
            Assert.AreNotEqual(null, ts.state.StopOrder);
            Assert.AreEqual(true, ts.state.LimitOrder.CancelSent);
            ts.tex.DeadifyOrder(ts.state.LimitOrder.MyID, ts.state.LimitOrder.Condition.Shares);
            new PrivateObject(ts.runner).Invoke("RunOne");
            Assert.AreNotEqual(null, ts.state.LimitOrder.Dead);
            //Gotta Cancel limit order 
        }
        [TestMethod()]
        public void OpenOrderDeadTest()
        {
            BlockingCollection<Packet> queue = new BlockingCollection<Packet>();
            TestExecutionServer tex = new TestExecutionServer(queue);
            TestQuoteServer tqs = new TestQuoteServer();
            TestTimeProvider ttp = new TestTimeProvider();
            StrategyRunner runner = new StrategyRunner(tex, tqs, ttp, queue);
            strat str = new Tests.strat();
            runner.AddStrategy(str, new string[] { "AAPL" });
            //run.queue.Add(MDPackets.Tests.TradeReportPacketTests.getPacket());
            StrategyStateCollection[] stratColls = runner.GetStrategyStateCollections();
            //Assert.AreEqual(stratColls.Count(), 1);
            StrategyState state = stratColls[0].StrategyStates["AAPL"];
            state.Stock = new Stock("AAPL",ttp);
            new PrivateObject(state.Stock).SetField("bid", 100);
            ttp.TimeToReturn = DateTime.Now.Date.AddHours(8.49);
            state.Position = 100;
            state.OpenOrder = new Order() { TheirID = 12345 };
            //packetType = PacketType.ORDER_DEAD;
            //symbol = toks[1];
            //ClientID = Int32.Parse(toks[2]);
            //Side = toks[3][0];
            //Quantity = Int32.Parse(toks[4]);
            //OpenQuantity = Int32.Parse(toks[5]);
            runner.queue.Add(new OrderDead("DEAD|AAPL|12345|B|100|0".Split('|')));
            TestExecutionServer.orderID = 444;
            new PrivateObject(runner).Invoke("RunOne");
            Assert.AreNotEqual(null, state.LimitOrder, "No Limit Order Created");
            Assert.AreEqual(444, state.LimitOrder.MyID, "Limit Order MyId Wrong");
            new PrivateObject(runner).Invoke("RunOne");
        }
        public void LimitOrderDeadFillTest()
        {
            BlockingCollection<Packet> queue = new BlockingCollection<Packet>();
            TestExecutionServer tex = new TestExecutionServer(queue);
            TestQuoteServer tqs = new TestQuoteServer();
            TestTimeProvider ttp = new TestTimeProvider();
            StrategyRunner runner = new StrategyRunner(tex, tqs, ttp, queue);
            strat str = new Tests.strat();
            runner.AddStrategy(str, new string[] { "AAPL" });
            //run.queue.Add(MDPackets.Tests.TradeReportPacketTests.getPacket());
            StrategyStateCollection[] stratColls = runner.GetStrategyStateCollections();
            //Assert.AreEqual(stratColls.Count(), 1);
            StrategyState state = stratColls[0].StrategyStates["AAPL"];
            state.Stock = new Stock("AAPL",ttp);
            new PrivateObject(state.Stock).SetField("bid", 100);
            ttp.TimeToReturn = DateTime.Now.Date.AddHours(8.49);
            state.Position = 100;
            state.LimitOrder = new Order() { TheirID = 12345 };
            //packetType = PacketType.ORDER_DEAD;
            //symbol = toks[1];
            //ClientID = Int32.Parse(toks[2]);
            //Side = toks[3][0];
            //Quantity = Int32.Parse(toks[4]);
            //OpenQuantity = Int32.Parse(toks[5]);
            runner.queue.Add(new OrderDead("DEAD|AAPL|12345|S|100|0".Split('|')));
            new PrivateObject(runner).Invoke("RunOne");
        }
        //[TestMethod()]
        //public void LimitOrderDeadFillTest()
        //{
        //    BlockingCollection<Packet> queue = new BlockingCollection<Packet>();
        //    TestExecutionServer tex = new TestExecutionServer();
        //    TestQuoteServer tqs = new TestQuoteServer();
        //    TestTimeProvider ttp = new TestTimeProvider();
        //    StrategyRunner runner = new StrategyRunner(tex, tqs, ttp, queue);
        //    strat str = new Tests.strat();
        //    runner.AddStrategy(str, new string[] { "AAPL" });
        //    //run.queue.Add(MDPackets.Tests.TradeReportPacketTests.getPacket());
        //    StrategyStateCollection[] stratColls = runner.GetStrategyStateCollections();
        //    //Assert.AreEqual(stratColls.Count(), 1);
        //    StrategyState state = stratColls[0].StrategyStates["AAPL"];
        //    state.Stock = new Stock();
        //    state.Stock.Symbol = "AAPL";
        //    new PrivateObject(state.Stock).SetField("bid", 100);
        //    ttp.TimeToReturn = DateTime.Now.Date.AddHours(8.49);
        //    state.Position = 100;
        //    state.LimitOrder = new Order() { TheirID = 12345 };
        //    //packetType = PacketType.ORDER_DEAD;
        //    //symbol = toks[1];
        //    //ClientID = Int32.Parse(toks[2]);
        //    //Side = toks[3][0];
        //    //Quantity = Int32.Parse(toks[4]);
        //    //OpenQuantity = Int32.Parse(toks[5]);
        //    runner.queue.Add(new OrderDead("DEAD|AAPL|12345|S|100|0".Split('|')));
        //    new PrivateObject(runner).Invoke("RunOne");
        //    Assert.AreNotEqual(null, state.LimitOrder);
        //    Assert.AreNotEqual(null, state.LimitOrder.Dead);
        //    Assert.AreEqual(0, state.LimitOrder.Dead.OpenQuantity);
        //    Assert.AreEqual('S', state.LimitOrder.Dead.Side);
        //}
        [TestMethod()]
        public void LimitOrderDeadCancelTest()
        {
            BlockingCollection<Packet> queue = new BlockingCollection<Packet>();
            TestExecutionServer tex = new TestExecutionServer(queue);
            TestQuoteServer tqs = new TestQuoteServer();
            TestTimeProvider ttp = new TestTimeProvider();
            StrategyRunner runner = new StrategyRunner(tex, tqs, ttp, queue);
            strat str = new Tests.strat();
            runner.AddStrategy(str, new string[] { "AAPL" });
            //run.queue.Add(MDPackets.Tests.TradeReportPacketTests.getPacket());
            StrategyStateCollection[] stratColls = runner.GetStrategyStateCollections();
            //Assert.AreEqual(stratColls.Count(), 1);
            StrategyState state = stratColls[0].StrategyStates["AAPL"];
            state.Stock = new Stock("AAPL",ttp);
            new PrivateObject(state.Stock).SetField("bid", 100);
            ttp.TimeToReturn = DateTime.Now.Date.AddHours(8.49);
            state.Position = 100;
            state.LimitOrder = new Order() { TheirID = 12345 };
            //Assert.Fail("A");
            Assert.AreEqual(null, state.LimitOrder.Dead);
            runner.queue.Add(new OrderDead("DEAD|AAPL|12345|S|100|100".Split('|')));
            new PrivateObject(runner).Invoke("RunOne");
            Assert.AreNotEqual(null, state.LimitOrder);
            Assert.AreNotEqual(null, state.LimitOrder.Dead);
            Assert.AreEqual(100, state.LimitOrder.Dead.OpenQuantity);
            Assert.AreEqual('S', state.LimitOrder.Dead.Side);
        }
        [TestMethod()]
        public void TradeReportBuildCandle()
        {
            BlockingCollection<Packet> queue = new BlockingCollection<Packet>();
            TestExecutionServer tex = new TestExecutionServer(queue);
            TestQuoteServer tqs = new TestQuoteServer();
            TestTimeProvider ttp = new TestTimeProvider();
            StrategyRunner runner = new StrategyRunner(tex, tqs, ttp, queue);
            strat str = new Tests.strat();
            runner.AddStrategy(str, new string[] { "AAPL" });
            //run.queue.Add(MDPackets.Tests.TradeReportPacketTests.getPacket());
            StrategyStateCollection[] stratColls = runner.GetStrategyStateCollections();
            //Assert.AreEqual(stratColls.Count(), 1);
            //StrategyState state = stratColls[0].StrategyStates["AAPL"];
            //state.Stock = new Stock("AAPL",ttp);
            //new PrivateObject(state.Stock).SetField("bid", 100);
            ttp.TimeToReturn = DateTime.Now.Date + new TimeSpan(8,30,0);
            DateTime dt;
            dt = DateTime.Now.Date.Add(new TimeSpan(8, 30, 0));
            TradeReportPacket trp = BuildFakeData.BuildFakeTradeReportPacket("AAPL", dt, 123, 111.11, 100, dt);
            runner.queue.Add(trp);
            new PrivateObject(runner).Invoke("RunOne");
            StrategyState state = stratColls[0].StrategyStates["AAPL"];
            Assert.AreEqual(111.11, state.Stock.CandleManager.Get(0).Open);
            Assert.AreEqual(null, state.Stock.CandleManager.Get(1));
            ttp.TimeToReturn = DateTime.Now.Date + new TimeSpan(8, 31, 1);
            Assert.AreNotEqual(null, state.Stock.CandleManager.Get(1));
            dt = DateTime.Now.Date.Add(new TimeSpan(8, 30, 0));
            trp = BuildFakeData.BuildFakeTradeReportPacket("AAPL", dt, 123, 112.11, 100, dt);
            runner.queue.Add(trp);
            dt = DateTime.Now.Date.Add(new TimeSpan(8, 30, 1));
            trp = BuildFakeData.BuildFakeTradeReportPacket("AAPL", dt, 123, 110.11, 100, dt);
            runner.queue.Add(trp);
            dt = DateTime.Now.Date.Add(new TimeSpan(8, 30, 2));
            trp = BuildFakeData.BuildFakeTradeReportPacket("AAPL", dt, 123, 111.10, 100, dt);
            runner.queue.Add(trp);
            new PrivateObject(runner).Invoke("RunOne");
            new PrivateObject(runner).Invoke("RunOne");
            new PrivateObject(runner).Invoke("RunOne");
            ttp.TimeToReturn = DateTime.Now.Date + new TimeSpan(8, 31, 1);
            Assert.AreEqual(111.11, state.Stock.CandleManager.Get(1).Open);
            Assert.AreEqual(112.11, state.Stock.CandleManager.Get(1).High);
            Assert.AreEqual(110.11, state.Stock.CandleManager.Get(1).Low);
            Assert.AreEqual(111.10, state.Stock.CandleManager.Get(1).Close);
            ttp.TimeToReturn = DateTime.Now.Date + new TimeSpan(8, 40, 1);
            Assert.AreEqual(111.11, state.Stock.CandleManager.Get(1).Open);
            Assert.AreEqual(112.11, state.Stock.CandleManager.Get(1).High);
            Assert.AreEqual(110.11, state.Stock.CandleManager.Get(1).Low);
            //Assert.AreEqual(111.11, state.Stock.CandleManager.Get(0).Open);
            //state.Position = 100;
        }
    }
}