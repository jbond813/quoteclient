using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDPackets
{
    public class BookModifyQuotePacket : Packet
    {
        public BookModifyQuotePacket(ushort len, ushort pt, byte[] pl)
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
        public string Symbol { get { return ASCIIEncoding.ASCII.GetString(rawPayload,2,8).TrimEnd((char)0); } set { } }
        public Int64 ReferenceNumber { get { return BitConverter.ToInt64(rawPayload,14); } set { }}
        public double Price { get { return getPrice(rawPayload, 22); } set { } }
        public Int32 Size { get { return BitConverter.ToInt32(rawPayload, 30); } set { } }
        //public Int32 MMID { get { return BitConverter.ToInt32(rawPayload, 34); } set { } }
        public string MMID { get { return ASCIIEncoding.ASCII.GetString(rawPayload,34,4); } set { } }
        public DateTime Time { get { return getTime(rawPayload,38); } set { } }
        public byte BookID { get { return rawPayload[42]; } set { } }
        public byte QuoteCondition { get { return rawPayload[43]; } set { } }
        public Int16 Flags { get { return BitConverter.ToInt16(rawPayload, 44); } set { } }

        // Using a DependencyProperty as the backing store for ReferenceNumber.  This enables animation, styling, binding, etc...



        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...




        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Size).Append("ModifyQuote ").Append(Symbol).Append("@").Append(Price.ToString("0.00")).Append(" ");
            sb.Append(packetType).Append(" ").Append(ReferenceNumber).Append(" ").Append(MMID).Append(" ");
            sb.Append(Time.ToString("HH:mm:ss.fff")).Append(" ");
            sb.Append(BookID).Append(" ");
            sb.Append(QuoteCondition).Append(" ");
            sb.Append(Flags);
            return sb.ToString();
            //StringBuilder sb = new StringBuilder();
            //sb.Append("PacketType=");
            //sb.Append(packetType);
            //sb.Append(" ReferenceNumber=");
            //sb.Append(ReferenceNumber);
            //sb.Append(" Symbol=");
            ////sb.Append(Symbol);
            //sb.Append(" Price=");
            //sb.Append(Price.ToString("0.00"));
            //sb.Append(" Ask=");
            //sb.Append(Size.ToString());
            //sb.Append(" MMID=");
            //sb.Append(MMID);
            //sb.Append(" Time=");
            //sb.Append(Time.ToString("HH:mm:ss.fff"));
            //sb.Append(" BookID=");
            //sb.Append(BookID);
            //sb.Append(" QuoteCondition=");
            //sb.Append(QuoteCondition);
            //sb.Append(" Flags=");
            //sb.Append(Flags);
            //return sb.ToString();
        }
    }
}
