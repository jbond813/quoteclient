using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketData
{
    public interface IQuoteServer
    {
        void Subscribe(string Symbol);
    }
}
