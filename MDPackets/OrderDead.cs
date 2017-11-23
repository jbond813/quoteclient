using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDPackets
{
    public class OrderDead : Packet
    {
        //public string Symbol;
        public int ClientID;
        public char Side;
        public int Quantity;
        public int OpenQuantity;
        private string symbol;
        public override string Symbol { get { return symbol; }}

        public OrderDead(string[] toks)
        {
            packetType = PacketType.ORDER_DEAD;
            symbol = toks[1];
            ClientID = Int32.Parse(toks[2]);
            Side = toks[3][0];
            Quantity = Int32.Parse(toks[4]);
            OpenQuantity = Int32.Parse(toks[5]);
        }
    }
}
