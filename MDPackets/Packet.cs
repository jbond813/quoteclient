using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDPackets
{
    public class Packet
    {
        public static Stream audit = null;
        protected DateTime getTime(byte[] b, int off)
        {
            int t = BitConverter.ToInt32(b, off);
            int tmp;
            tmp = t;
            int h = tmp / 3600000;
            tmp -= (h * 3600000);
            int m = tmp / 60000;
            tmp -= (m * 60000);
            int s = tmp / 1000;
            int f = tmp % 1000;
            string rvx = h.ToString("00:") + m.ToString("00:") + s.ToString("00") + "." + f.ToString("000");
            DateTime rv = DateTime.Today;
            rv = rv.Add(new TimeSpan(h, m, s));
            rv = rv.AddMilliseconds(f);
            return rv;
            //return rv;
        }
        public double getPrice(byte[] b, int off)
        {
            double rv;
            int d = BitConverter.ToInt32(b, off);
            double c = (double)BitConverter.ToInt32(b, off + 4);
            rv = d + (c / 1000000000);
            return rv;
        }
        public enum PacketType
        {
            M_SYSTEM_EVENT = 1,//MARKET_DATA_START + 1,

            // Book related messages
            M_BOOK_NEW_QUOTE,
            M_BOOK_MODIFY_QUOTE,
            M_BOOK_DELETE_QUOTE,
            M_BOOK_ORDER_EXECUTED,
            M_BOOK_ORDER_EXECUTED_WITH_PRICE,
            M_BOOK_ORDER_CANCELED,
            M_BOOK_TRADE,
            M_BOOK_CROSS_TRADE,

            // Stock related messages
            M_STOCK_DIRECTORY,
            M_STOCK_TRADING_ACTION,
            M_STOCK_IMBALANCE_INDICATOR,

            // Refresh request and response
            M_REQ_REFRESH_SYMBOL,
            M_RESP_REFRESH_SYMBOL,

            // Used internaly to notify Aggregated Book
            //    M_QUOTE_SIZE_CHANGE,
            M_RESET_BOOK = 16,

            // Level1
            M_LEVEL1,

            // Trades
            M_TRADE_REPORT,
            M_TRADE_CANCEL_ERROR,
            M_TRADE_CORRECTION
        };
        public ushort length;
        public PacketType packetType;
        public byte[] rawPayload;
        public override string ToString()
        {
            return "packetType " + packetType + " packetLength " + length;
        }
        public Packet() { }
        public static Packet GetPacket(Stream str)
        {
            byte[] lbuff = new byte[2];
            int count = 0;
            ushort length = 0;
            byte[] pl = null;
            do
            {
                int lc = 0;
                do
                {
                    int c = str.Read(lbuff, lc, 2 - lc);
                    if (audit != null) audit.Write(lbuff, lc, 2 - lc);
                    lc += c;
                } while (lc < 2);
                
                length = (ushort)(BitConverter.ToUInt16(lbuff, 0) - 2);
                int offset = 0;
                pl = new byte[length];
                count = 0;
                while (count < length)
                {
                    int c = str.Read(pl, offset, length - count);
                    if (audit != null) audit.Write(pl, offset, length - count);
                    count += c;
                    offset = count;
                }
                if(length < 2)
                {
                    Console.WriteLine("dropped packet");
                    //for(;;)
                    //{
                    //    Console.
                    //}
                }
            } while (length < 2);
            ushort pt = BitConverter.ToUInt16(pl, 0);
//            Console.WriteLine((PacketType)pt);
            if (pt != 501 && pt != 505 && pt != 504)
            {
                //Console.WriteLine((PacketType)pt);
            }
            Packet rv = null;
            // packetType = (PacketType)pt;
            switch ((PacketType)pt)
            {
                case PacketType.M_RESP_REFRESH_SYMBOL:
                    rv = new RespRefreshSymbolPacket(length, pt, pl);
                    break;
                case PacketType.M_LEVEL1:
                    rv = new L1Packet(length,pt,pl);
                    break;
                case PacketType.M_BOOK_MODIFY_QUOTE:
                    rv = new BookModifyQuotePacket(length, pt, pl);
                    break;
                case PacketType.M_BOOK_NEW_QUOTE:
                    rv = new BookNewQuotePacket(length, pt, pl);
                    break;
                case PacketType.M_BOOK_DELETE_QUOTE:
                    rv = new BookDeleteQuotePacket(length, pt, pl);
                    break;
                case PacketType.M_STOCK_IMBALANCE_INDICATOR:
                    rv = new StockImbalanceIndicator(length, pt, pl);
                    break;
                case PacketType.M_BOOK_ORDER_CANCELED:
                    rv = new BookOrderCancelledPacket(length, pt, pl);
                    break;
                case PacketType.M_BOOK_ORDER_EXECUTED:
                    rv = new BookOrderExecutedPacket(length, pt, pl);
                    break;
                case PacketType.M_TRADE_REPORT:
                    rv = new TradeReportPacket(length, pt, pl);
                    break;
                default:
                    return new Packet(length, pt, pl);
            }

            return rv;
        }
        protected Packet(ushort len, ushort pt, byte[] pl)
        {
            length = len;
            packetType = (PacketType)pt;
            rawPayload = pl;
        }
        //public Packet(Stream str)
        //{
        //    byte[] lbuff = new byte[2];
        //    str.Read(lbuff, 0, 2);
        //    length = (ushort)(BitConverter.ToUInt16(lbuff, 0) - 2);
        //    int offset = 0;
        //    rawPayload = new byte[length];
        //    int count = 0;
        //    while(count < length)
        //    {
        //        count += str.Read(rawPayload, offset, length - count);
        //        offset = count;
        //    }
        //    ushort pt =  BitConverter.ToUInt16(rawPayload,0);
        //    packetType = (PacketType)pt;
        //    //Console.Write("packet length " + length);
        //    //int count = str.Read(r, offset, length);
        //    //packetType = BitConverter.ToUInt16(r, 0);
        //    //Console.WriteLine(" read " + count + " bytes packettype " + packetType);
        //    //if (count != length)
        //    //{
        //    //    partialPacket = true;
        //    //    length -= count;
        //    //    offset = count;
        //    //}
        //    //else
        //    //{
        //    //    partialPacket = false;
        //    //}

        //    //rawPayload = new byte[l];

        //}
    }
}
