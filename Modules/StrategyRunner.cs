using MarketData;
using MDPackets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingInterfaces;

namespace Modules
{
    static class ExtensionMethods
    {
        static StrategyState[] GetValidStrategyStates(Packet p, IEnumerable<StrategyStateCollection> coll)
        {
            List<StrategyState> l = new List<StrategyState>();
            foreach (StrategyStateCollection sstatec in coll)
            {
                if (sstatec.StrategyStates.ContainsKey(p.Symbol))
                {
                    l.Add(sstatec.StrategyStates[p.Symbol]);
                }
            }
            return l.ToArray();
        }
        public static void Process(this OrderDead od, List<StrategyStateCollection> strategyList, IExecutionServer es)
        {
            foreach (StrategyState state in GetValidStrategyStates(od, strategyList))
            {
                if (state.OpenOrder != null && od.ClientID == state.OpenOrder.TheirID)
                {
                    if (state.Position != 0)
                    {

                        OrderCondition oc = state.strategy.GetLimitConditions(state);
                        state.LimitOrder = es.SendOrder(oc, state.Stock);
                        //state.LimitOrder = new Order() { Condition = oc };
                        //state.LimitOrder.MyID = es.SendOrder(oc.Side, state.Stock.Symbol, oc.Limit, state.Stock.Bid, state.Stock.Ask, oc.Tif.ToString(), oc.Shares);
                    }
                }
                else if (state.LimitOrder != null && od.ClientID == state.LimitOrder.TheirID)
                {
                    state.LimitOrder.Dead = od;
                }
                else if (state.StopOrder != null && od.ClientID == state.StopOrder.TheirID)
                {

                }
            }
        }
        public static void Process(this OrderExecuted oe, List<StrategyStateCollection> strategyList)
        {
            foreach (StrategyState state in GetValidStrategyStates(oe, strategyList))
            {
                if ((state.OpenOrder != null && oe.ClientOrderID == state.OpenOrder.TheirID)
                    ||(state.LimitOrder != null && oe.ClientOrderID == state.LimitOrder.TheirID)
                    ||( state.StopOrder != null && oe.ClientOrderID == state.StopOrder.TheirID))
                {
                    if (oe.Side == 'B')
                    {
                        state.Position += oe.ExecutionQuantity;
                    }
                    else
                    {
                        state.Position -= oe.ExecutionQuantity;
                    }

                }
            }
        }
        public static void Process(this OrderSent os, List<StrategyStateCollection> strategyList)
        {
            foreach (StrategyState state in GetValidStrategyStates(os, strategyList))
            {
                if (os.MyID == state.OpenOrder.MyID)
                {
                    state.OpenOrder.TheirID = os.TheirID;
                }
                else if (os.MyID == state.LimitOrder.MyID)
                {
                    state.LimitOrder.TheirID = os.TheirID;
                }
                else if (os.MyID == state.StopOrder.MyID)
                {
                    state.StopOrder.TheirID = os.TheirID;
                }
            }
        }
        public static void Process(this TradeReportPacket tr, List<StrategyStateCollection> strategyList,Dictionary<string,Stock> dict, IExecutionServer es)
        {
            if(dict.ContainsKey(tr.Symbol))
            {
                dict[tr.Symbol].LastTrade = tr;
                dict[tr.Symbol].CandleManager.Update(tr);
            }
            foreach (StrategyState state in GetValidStrategyStates(tr, strategyList))
            {
                OrderCondition oc = null;
                //if (state.OpenOrder == null)
                //{
                //    oc = state.strategy.GetEntryCondition(state);
                //}
                if (state.Position != 0 && state.StopOrder == null)
                {
                    oc = state.strategy.GetStopConditions(state);
                    if(oc != null)
                    {
                        state.StopOrder = es.SendOrder(oc, state.Stock);
                        if (state.LimitOrder != null)
                        {
                            es.CancelOrder(state.LimitOrder);
                        }
                        //state.StopOrder = new Order()
                        //{
                        //    Condition = oc,
                        //    MyID = es.SendOrder(oc.Side, state.Stock.Symbol, oc.Limit, state.Stock.Bid, state.Stock.Ask, oc.Tif.ToString(), oc.Shares),
                        //    Symbol = state.Stock.Symbol
                        //};
                    }
                }
            }
        }
    }
    public class StrategyRunner
    {
        public StrategyRunner(IExecutionServer es, IQuoteServer qs, ITimeProvider timeProvider, BlockingCollection<Packet> queue)
        {
            this.qs = qs;
            this.es = es;
            this.timeProvider = timeProvider;
            this.queue = queue;
        }
        IQuoteServer qs;
        IExecutionServer es;
        List<StrategyStateCollection> strategyList = new List<StrategyStateCollection>();
        public BlockingCollection<Packet> queue;
        public ITimeProvider timeProvider;
        public Dictionary<string, Stock> Stocks = new Dictionary<string, Stock>();
        public void AddStrategy(IStrategy strat, string[] candidateSymbols)
        {
            List<Stock> stockList = new List<Stock>();
            foreach(string symbol in candidateSymbols)
            {
                if(!Stocks.ContainsKey(symbol))
                {
                    Stock st = new Stock(symbol,timeProvider);
                    Stocks[symbol] = st;
                    qs.Subscribe(symbol);
                    stockList.Add(st);
                }
            }
            strategyList.Add(new StrategyStateCollection(strat, stockList.ToArray()));
        }
        public bool Shutdown = false;
        public void Run()
        {
            for (; !Shutdown;)
            {
                RunOne();
            }
        }
        void RunOne()
        {
            //return "AAA";
            //int ordID = es.SendOrder('B',"AAPL",172.30,172.20,173.00,"OPG",1);
            //qs.Subscribe("AAPL");
            //for (;!Shutdown;)
            //{
            DateTime now = timeProvider.GetCurrentTime();
            foreach (StrategyStateCollection ssc in strategyList)
            {
                if (!ssc.hasEvaluatedEntry)
                {
                    foreach (StrategyState ss in ssc.StrategyStates.Values)
                    {
                        if (now > ssc.Strategy.GetOpenTime(ss))
                        {
                            ssc.hasEvaluatedEntry = true;
                            OrderCondition oc = ssc.Strategy.GetEntryCondition(ss);
                            if (oc.Shares != 0)
                            {
                                //ss.OpenOrder = new Order();
                                //ss.OpenOrder.MyID = es.SendOrder(oc.Side, ss.Stock.Symbol, oc.Limit, ss.Stock.Bid, ss.Stock.Ask, oc.Tif.ToString(), oc.Shares);
                                ss.OpenOrder = es.SendOrder(oc, ss.Stock);
                            }
                        }
                    }
                }
            }
            Packet p;
            if (queue.TryTake(out p, 5))
            {
                switch (p.packetType)
                {
                    case Packet.PacketType.ORDER_SENT:
                        OrderSent osp = (OrderSent)p;
                        osp.Process(strategyList);
                        break;
                    case Packet.PacketType.ORDER_EXECUTED:
                        OrderExecuted oe = (OrderExecuted)p;
                        oe.Process(strategyList);
                        break;
                    case Packet.PacketType.M_TRADE_REPORT:
                        TradeReportPacket trp = (TradeReportPacket)p;
                        trp.Process(strategyList,Stocks,es);
                        break;
                    case Packet.PacketType.ORDER_DEAD:
                        OrderDead od = (OrderDead)p;
                        od.Process(strategyList,es);
                        break;
                }
            }
            //}
        }
        public StrategyStateCollection[] GetStrategyStateCollections()
        {
            return strategyList.ToArray();
        }
    }
}
