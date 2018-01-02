using MDPackets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradingInterfaces;

namespace Modules
{
    public class CandleManager
    {
        //private Dictionary<int, Candle> candles = new Dictionary<int, Candle>();
        List<Candle> candles = new List<Candle>();
        public void Update(TradeReportPacket trp)
        {
            if (candles.Count == 0 || candles.Last().Minute != trp.Minute())
            {
                //Assert.Fail("S " + min);
                candles.Add(new Candle(trp));
                //Assert.AreEqual(1, candles.Count);
            }
            else candles.Last().Update(trp);
        }
        private ITimeProvider tp;
        public CandleManager(ITimeProvider tp)
        {
            this.tp = tp;
        }
        public Candle Get(int offset)
        {
            int curr = (int)Math.Floor(tp.GetCurrentTime().TimeOfDay.TotalMinutes);
            int min = curr - offset;
            Candle rv = null;
            for (int i = candles.Count - 1; i > -1; i--)
            {
                if (candles[i].Minute <= min)
                {
                    rv = candles[i];
                    break;
                }
            }
            return rv;
        }
    }
}
