using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDPackets
{
    public class OrderSent : Packet
    {
        //public String Symbol;
        public int MyID;
        public int TheirID;
        private string symbol;

        public override string Symbol
        {
            get { return symbol; }
        }

        public OrderSent(string[] toks)
        {
            packetType = PacketType.ORDER_SENT;
            symbol = toks[1];
            MyID = Int32.Parse(toks[2]);
            TheirID = Int32.Parse(toks[3]);
        }
    }
}
