using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDPackets
{
    public class BookOrderCancelledPacket : Packet
    {
        public BookOrderCancelledPacket(ushort len, ushort pt, byte[] pl)
        {
            rawPayload = pl;
            length = len;
            packetType = (PacketType)pt;
        }
        //      const unsigned __int64& referenceNumber,
        //const unsigned int& priceDollars,
        //const unsigned int& priceFraction,
        //const unsigned int& size,
        //const unsigned int& mmid,
        //const unsigned int& millisecond,
        //const unsigned char& bookID,//	: 8;
        //const unsigned char& quoteCondition,//	: 8;
        //const unsigned short& flags


        //private Int64 referenceNumber;
        public DateTime Time { get { return getTime(rawPayload, 2); } set { } }
        public string Symbol { get { return ASCIIEncoding.ASCII.GetString(rawPayload, 6, 8).TrimEnd((Char)0); } set { } }
        public Int64 ReferenceNumber { get { return BitConverter.ToInt64(rawPayload, 14); } set { } }
        public override string ToString()
        {
            return new StringBuilder().Append("Book Order Cancelled ").Append(Time.ToString("HH:mm:ss.fff")).Append(" ").Append(Symbol).Append(" ").Append(ReferenceNumber).ToString();
        }
    }
}
