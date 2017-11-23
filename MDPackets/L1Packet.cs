using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDPackets
{
    public class L1Packet : Packet
    {
        //public L1Packet(Packet b)
        //{
        //    length = b.length;
        //    packetType = b.packetType;
        //    rawPayload = b.rawPayload;
        //}
        public L1Packet(ushort len, ushort pt, byte[] pl)
        {
            rawPayload = pl;
            length = len;
            packetType = (PacketType)pt;
        }
        //private DateTime time;

        private double bid;

        public double Bid
        {
            get {
                bid = getPrice(rawPayload, 14);
                return bid;
            }
        }
        private double ask;

        public double Ask
        {
            get {
                ask = getPrice(rawPayload, 22);
                return ask; 
            }
        }
        private int bidSize;

        public int BidSize
        {
            get {
                bidSize = BitConverter.ToInt32(rawPayload, 30);
                return bidSize;
            }
        }
        private int askSize;

        public int AskSize
        {
            get {
                askSize = BitConverter.ToInt32(rawPayload, 34);
                return askSize;
            }
        }
        private String symbol;

        public override String Symbol
        {
            get {
                symbol = ASCIIEncoding.ASCII.GetString(rawPayload, 6, 8).TrimEnd((Char)0);
                return symbol;
            }
        }
        //private DateTime myVar;

        public DateTime Time
        {
            get { return getTime(rawPayload,2); }
        }





        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("PacketType=");
            sb.Append(packetType);
            sb.Append(" Symbol=");
            sb.Append(Symbol);
            sb.Append(" Bid=");
            sb.Append(Bid.ToString("0.00"));
            sb.Append(" Ask=");
            sb.Append(Ask.ToString("0.00"));
            sb.Append(" BidSize=");
            sb.Append(BidSize);
            sb.Append(" AskSize=");
            sb.Append(AskSize);
            sb.Append(" Time=");
            sb.Append(Time);
            return sb.ToString();
        }
    }
}
