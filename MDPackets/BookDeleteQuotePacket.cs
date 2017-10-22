using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDPackets
{
    public class BookDeleteQuotePacket : Packet
    {
        public BookDeleteQuotePacket(ushort len, ushort pt, byte[] pl)
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
        public char Side
        {
            get
            {
                char c = (char)rawPayload[26];
                return c == 0 ? 'B' : 'S';
            }
            set { }
        }
        public char BookID { get { return ASCIIEncoding.ASCII.GetChars(rawPayload, 24, 1)[0]; } set { } }

        public override string ToString()
        {

            StringBuilder sb = new StringBuilder();
            if(ReferenceNumber == 171489)
            {

            }
            sb.Append(Side).Append("DeleteQuote ").Append(Symbol).Append(" ");
            sb.Append(packetType).Append(" ").Append(ReferenceNumber).Append(" ");
            sb.Append(Time.ToString("HH:mm:ss.fff")).Append(" ");
            sb.Append(BookID).Append(" ");
            return sb.ToString();
        }
    }
}
