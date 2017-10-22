using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDPackets
{
    public class QuoteHolder
    {
        public L1Packet L1;
        public List<RespRefreshSymbolPacket> Resp = new List<RespRefreshSymbolPacket>();
        public double PrevClose;
        public int Volume = -1;
        public double ATR;
        public int ADV;
        public bool hasPosition = false;
        public Candle Current;
        public Candle P1;
        public bool OrderSent = false;
        public int OpenShares = 0;
        public DateTime ExecutionTime;
        public Double ExecutionPrice = 0;
        public double Stop = 0;
        public static StreamWriter Logger;
        public bool Closing = false;
        public void PostExec(string Symbol,int shares, double Price)
        {
            ExecutionPrice = Price;
            OpenShares += shares;
            Stop = PrevClose + .02;
            ExecutionTime = DateTime.Now;
            Logger.WriteLine($"Execution Posted {Symbol} {shares} {Price.ToString("0.00")} stop {Stop}");
        }
        public class Candle
        {
            public DateTime time;
            public double High;
            public double Low;
            public bool Update(TradeReportPacket p)
            {
                bool closed = false;
                if(p.Time.Minute != time.Minute && p.Time.Hour != time.Hour)
                {
                    closed = true;
                }
                else
                {
                    if (p.Price > High) High = p.Price;
                    if (p.Price < Low) Low = p.Price;
                }
                return closed;
            }
            public override string ToString()
            {
                string h = High == Double.MinValue ? "0" : High.ToString("0.00");
                string l = Low == Double.MaxValue ? "0" : Low.ToString("0.00");
                return $"{time.ToString("HH:mm:ss")} {h} {l}";
            }
        }
        public QuoteHolder(string s)
        {
            Current = new Candle() { time = DateTime.Now, High = Double.MinValue, Low = double.MaxValue };
            string[] toks = s.Split(' ');
            PrevClose = Double.Parse(toks[3]);
            ATR = Double.Parse(toks[1]);
            ADV = Int32.Parse(toks[2]);
        }
        public int ProcessTrade(TradeReportPacket p)
        {
            Logger.WriteLine($"processing {p}");
            int qtyToSell = 0;
            Volume += p.Size;
            if(Current.Update(p))
            {
                P1 = Current;
                Current = new Candle() { time = DateTime.Now, High  = Double.MinValue, Low = Double.MaxValue};
                Current.Update(p);
                //Console.WriteLine($"{p.Symbol} {P1}");
            }
            if(OpenShares > 0 && (DateTime.Now - ExecutionTime).TotalSeconds > 10) //TEST ONLY should be 75
            {
                if (P1 != null)
                {
                    if (P1.Low != Double.MaxValue)
                    {
                        double how = P1.Low - (ATR * .03);
                        if(how > Stop)
                        {
                            Logger.WriteLine($"{p.Symbol} stop moving from {Stop} to {how}");
                            Stop = how;
                        }
                        else
                        {
                            if (p.Price < Stop)
                            {
                                qtyToSell = OpenShares;
                                OpenShares = 0;
                            }
                        }
                    }
                    else
                    {
                        Logger.WriteLine($"P1 HAD NO TRADES {p.Symbol}");
                    }
                }
                else
                {
                    Logger.WriteLine($"P1 was NULL {p.Symbol}");
                }
            }
            return qtyToSell;
        }
    }
}
