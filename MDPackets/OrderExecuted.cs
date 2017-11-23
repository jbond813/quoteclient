using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDPackets
{
    public class OrderExecuted : Packet
    {
        private string symbol;
        public double Price;
        public int ExecutionQuantity;
        public int PositionQuantity;
        public char Side;
        public int ClientOrderID;
        public override string Symbol { get { return symbol; } }
        public OrderExecuted(string[] toks)
        {
            packetType = PacketType.ORDER_EXECUTED;
            symbol = toks[1];
            Price = double.Parse(toks[2]);
            ExecutionQuantity = Int32.Parse(toks[3]);
            PositionQuantity = Int32.Parse(toks[4]);
            Side = toks[5][0];
            ClientOrderID = Int32.Parse(toks[6]);
        }
    }
}
