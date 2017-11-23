using MarketData;
using MDPackets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static void Process(this OrderDead od,List<StrategyStateCollection> strategyList)
        {
            foreach (StrategyState state in GetValidStrategyStates(od,strategyList))
            {
                if (od.ClientID == state.OpenOrder.TheirID)
                {

                }
                else if (od.ClientID == state.LimitOrder.TheirID)
                {

                }
                else if (od.ClientID == state.StopOrder.TheirID)
                {

                }
            }
        }
        public static void Process(this OrderExecuted oe, List<StrategyStateCollection> strategyList)
        {
            foreach (StrategyState state in GetValidStrategyStates(oe, strategyList))
            {
                if(oe.ClientOrderID == state.OpenOrder.TheirID || oe.ClientOrderID == state.LimitOrder.TheirID || oe.ClientOrderID == state.StopOrder.TheirID)
                {
                    if(oe.Side == 'B')
                    {
                        state.Position += oe.ExecutionQuantity;
                    } else
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
        public static void Process(this TradeReportPacket tr, List<StrategyStateCollection> strategyList)
        {
            foreach (StrategyState state in GetValidStrategyStates(tr, strategyList))
            {
                OrderCondition oc = null;
                if (state.OpenOrder == null)
                {
                    oc = state.strategy.GetEntryCondition(state);
                }
                if(state.Position != 0 && state.StopOrder != null)
                {
                    oc = state.strategy.GetStopConditions(state);
                }
            }
        }
    }
    public class StrategyRunner
    {
        List<StrategyStateCollection> strategyList = new List<StrategyStateCollection>();
        public BlockingCollection<Packet> queue = new BlockingCollection<Packet>();
        public void AddStrategy(IStrategy strat, string[] candidateSymbols)
        {
            strategyList.Add(new StrategyStateCollection(strat, candidateSymbols));
        }
        public void Run()
        {
            QuoteServer qs = new QuoteServer(queue);
            ExecutionServer es = new ExecutionServer(queue);
            //int ordID = es.SendOrder('B',"AAPL",172.30,172.20,173.00,"OPG",1);
            //qs.Subcribe("AAPL");
            for(;;)
            {
                Packet p = queue.Take();
                switch(p.packetType)
                {
                    case Packet.PacketType.M_TRADE_REPORT:
                        TradeReportPacket trp = (TradeReportPacket)p;
                        trp.Process(strategyList);
                        break;
                    case Packet.PacketType.ORDER_DEAD:
                        OrderDead od = (OrderDead)p;
                        od.Process(strategyList);
                        break;
                }
            }
        }
    }
}
