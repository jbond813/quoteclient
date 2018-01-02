using MDPackets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingInterfaces;

namespace Modules
{
    public class Stock
    {
        public string Symbol;
        public CandleManager CandleManager;
        private ITimeProvider timeProvider;
        private double bid;
        public TradeReportPacket LastTrade;
        public double Bid
        {
            get { return bid; }
        }
        public double Ask
        {
            get { return 0; }
        }
        public int Volume
        {
            get { return 0; }
        }
        public Stock(string Symbol, ITimeProvider tp)
        {
            this.Symbol = Symbol;
            this.timeProvider = tp;
            this.CandleManager = new CandleManager(tp);
        }
        //public Candle[] Candles;
    }
}
