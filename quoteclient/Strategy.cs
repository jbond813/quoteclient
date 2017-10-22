using MDPackets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace quoteclient
{
    class Strategy
    {
        
        public virtual void TimedEntry(Dictionary<string,QuoteHolder> dict)
        {

        }
        public virtual void ProcessExecution(string exec, QuoteHolder qh)
        {

        }
        public virtual void ProcessTrade(QuoteHolder qh)
        {
        }
    }
}
