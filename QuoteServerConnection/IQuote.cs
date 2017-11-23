using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketData
{
    interface IQuote
    {
        double GetBid();
        double AskBid();
        double GetLast();
        int GetVolume();
        Candle GetCandle(int i);

    }
}
