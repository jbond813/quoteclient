using MDPackets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules
{
    public class Candle
    {
        public double High;
        public double Low;
        public double Open;
        public double Close;
        public int Volume;
        public int Minute;
        public Candle(TradeReportPacket trp)
        {
            High = Low = Open = Close = trp.Price;
            Volume = trp.Size;
            Minute = trp.Minute();
        }
        public void Update(TradeReportPacket trp)
        {
            if (trp.Price > High) High = trp.Price;
            if (trp.Price < Low) Low = trp.Price;
            Volume += trp.Size;
            Close = trp.Price;
        }

    }
}
