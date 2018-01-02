using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MDPackets;

namespace QuoteClientTester
{
    public class BuildFakeData
    {
        static void setPrice(byte[] b, int off, double price)
        {
            int d = BitConverter.ToInt32(b, off);
            int whole = Int32.Parse(price.ToString().Split('.')[0]);
            int frac = Int32.Parse(price.ToString("00.00").Split('.')[1]) * 10000000;
            byte[] a = BitConverter.GetBytes(whole);
            Array.Copy(a, 0, b, off, 4);
            a = BitConverter.GetBytes(frac);
            Array.Copy(a, 0, b, off + 4, 4);
        }
        static void setShort(byte[] b, int off, short h)
        {
            byte[] a = BitConverter.GetBytes(h);
            Array.Copy(a, 0, b, off, 2);
        }

        static void setInt(byte[] b, int off, int i)
        {
            byte[] a = BitConverter.GetBytes(i);
            Array.Copy(a, 0, b, off, 4);
        }
        static void setInt64(byte[] b, int off, Int64 i)
        {
            byte[] a = BitConverter.GetBytes(i);
            Array.Copy(a, 0, b, off, 8);
        }
        static void setTime(byte[] b, int off, DateTime dt)
        {
            int ms = (int)(dt - dt.Date).TotalMilliseconds;
            byte[] arr = BitConverter.GetBytes(ms);
            Array.Copy(arr, 0, b, off, 4);
        }
        static void setString(byte[] b, int off, int len, String s)
        {            
            byte[] arr = ASCIIEncoding.ASCII.GetBytes(s);
            for (int i = off; i < off + len; i++) b[i] = 0;
            Array.Copy(arr, 0, b, off, arr.Length > len ? len: arr.Length);
        }
        public static L1Packet BuildFakeL1Packet(string symbol, double bid, double Ask, int Bidsize, int Asksize, DateTime dt)
        {
            byte[] arr = new byte[38];
            Packet.PacketType pt = Packet.PacketType.M_LEVEL1;
            L1Packet rv = new L1Packet(38, (ushort)pt, arr);
            setShort(arr, 0, 38);
            setTime(arr, 2, dt);
            setString(arr, 6, 8, symbol);
            setPrice(arr, 14, bid);
            setPrice(arr, 22, Ask);
            setInt(arr, 30, Bidsize);
            setInt(arr, 34, Asksize);
            return rv;
        }
        public static TradeReportPacket BuildFakeTradeReportPacket(string symbol, DateTime time, Int64 refno, double Price, int Size, DateTime dt2)
        {
        //public DateTime Time { get { return getTime(rawPayload, 2); } set { } }
        //public override string Symbol { get { return ASCIIEncoding.ASCII.GetString(rawPayload, 6, 8).TrimEnd((Char)0); } }
        //public Int64 ReferenceNumber { get { return BitConverter.ToInt64(rawPayload, 14); } set { } }
        //public double Price { get { return getPrice(rawPayload, 22); } set { } }
        //public int Size { get { return BitConverter.ToInt32(rawPayload, 30); } set { } }
        //public string MMID { get { return ASCIIEncoding.ASCII.GetString(rawPayload, 34, 4); } set { } }
        //public DateTime Time2
        //{
        //    get { return getTime(rawPayload, 34); }
        //    set { }
        //}
            byte[] arr = new byte[46];
            Packet.PacketType pt = Packet.PacketType.M_TRADE_REPORT;
            TradeReportPacket rv = new TradeReportPacket(46, (ushort)pt, arr);
            setShort(arr, 0, 38);
            setTime(arr, 2, time);
            setString(arr, 6, 8, symbol);
            setInt64(arr, 14, refno);
            setPrice(arr, 22, Price);
            setInt(arr, 30, Size);
            //setString(arr, 34, 4, MMID);
            setTime(arr,38,dt2);
            return rv;
        }
    }
}
