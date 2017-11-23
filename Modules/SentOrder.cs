using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules
{
    public class SentOrder
    {
        public String Symbol;
        public int MyID;
        public int TheirID;
        public SentOrder(string message)
        {
            string[] toks = message.Split('|');
            Symbol = toks[1];
            MyID = Int32.Parse(toks[2]);
            TheirID = Int32.Parse(toks[3]);
        }
    }
}
