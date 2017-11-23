using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketData
{
    public class Candle
    {
        public DateTime Time
        {
            get { return DateTime.Now; }
        }
        public double High
        {
            get { return 0; }
        }
        public double Low
        {
            get { return 0; }
        }
        public double Open
        {
            get { return 0; }
        }
        public double Close
        {
            get { return 0; }
        }
    }
}
