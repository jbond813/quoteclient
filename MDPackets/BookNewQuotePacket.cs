using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDPackets
{
    public class BookNewQuotePacket : Packet
    {
        public BookNewQuotePacket(ushort len, ushort pt, byte[] pl)
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
        public string Symbol { get { return ASCIIEncoding.ASCII.GetString(rawPayload, 2,8).TrimEnd((Char)0); } set { } }
        public char Side { get {
                char c = (char)rawPayload[10];
                return  c==0?'B':'S';
            } set { } }
        public Int64 ReferenceNumber { get { return BitConverter.ToInt64(rawPayload, 14); } set { } }
        public double Price { get { return getPrice(rawPayload, 22); } set { } }
        public Int32 Size { get { return BitConverter.ToInt32(rawPayload, 30); } set { } }
        //public Int32 MMID { get { return BitConverter.ToInt32(rawPayload, 34); } set { } }
        public string MMID { get { return ASCIIEncoding.ASCII.GetString(rawPayload, 34, 4); } set { } }
        public DateTime Time { get { return getTime(rawPayload, 38); } set { } }
        public char BookID { get { return ASCIIEncoding.ASCII.GetChars(rawPayload, 42, 1)[0]; } set { } }
        public byte QuoteCondition { get { return rawPayload[43]; } set { } }
        public Int16 Flags { get { return BitConverter.ToInt16(rawPayload, 44); } set { } }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Side).Append("New Quote ").Append(Size).Append(" ").Append(Symbol).Append("@").Append(Price.ToString("0.00")).Append(" ");             
            sb.Append(packetType).Append(" ").Append(ReferenceNumber).Append(" ").Append(MMID).Append(" ");
            sb.Append(Time.ToString("HH:mm:ss.fff")).Append(" ");
            sb.Append(BookID).Append(" ");
            sb.Append(QuoteCondition).Append(" ");
            sb.Append(Flags);
            return sb.ToString();
        }
    }
}
