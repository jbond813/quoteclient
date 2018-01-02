using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDPackets
{
    public class TradeReportPacket : Packet
    {
        public TradeReportPacket(ushort len, ushort pt, byte[] pl)
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
        public override string Symbol { get { return ASCIIEncoding.ASCII.GetString(rawPayload, 6, 8).TrimEnd((Char)0); } }
        public Int64 ReferenceNumber { get { return BitConverter.ToInt64(rawPayload, 14); } set { } }
        public double Price { get { return getPrice(rawPayload, 22); } set { } }
        public int Size { get { return BitConverter.ToInt32(rawPayload,30); } set { } }
        //public string MMID { get { return ASCIIEncoding.ASCII.GetString(rawPayload, 34, 4); } set { } }
        public DateTime Time2
        {
            get { return getTime(rawPayload, 38); }
            set { }
        }
        public int Minute()
        {
            return (int)Math.Floor(Time2.TimeOfDay.TotalMinutes);
        }
        public override string ToString()
        {
            return new StringBuilder().Append("Trade Report ").Append(Time.ToString("HH:mm:ss.fff")).Append(" ").Append(Size).Append(" ")
                .Append(Symbol).Append("@")
                .Append(Price).ToString();
        }
    }
}
