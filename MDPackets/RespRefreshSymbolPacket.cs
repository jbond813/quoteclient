using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDPackets
{
    public class RespRefreshSymbolPacket : Packet
    {
        public class BookTrade
        {

        }
        static int totalPackets = 0;
        public Snapshot Snapshot = null;
        public RespRefreshSymbolPacket(ushort len, ushort pt, byte[] pl)
        {
            rawPayload = pl;
            length = len;
            packetType = (PacketType)pt;
            byte steps = rawPayload[14];
            byte flags = rawPayload[15];
            byte bookid = rawPayload[16];
            byte marketStatus = rawPayload[17];
            //Console.Write(Symbol + " ");
            int off = 18;
            if ((steps & SRS_DESCRIPTION) != 0)
            {
                string s = ASCIIEncoding.ASCII.GetString(rawPayload, off, 30);
                //Console.Write("Desc >>" + s + "<<");

            }
            if ((steps & SRS_ATTRIBUTES) != 0)
            {
                //Console.Write("Attributes ");

            }
            if ((steps & SRS_LEVEL1) != 0)
            {
                Snapshot = new Snapshot(this);
            }
            if ((steps & SRS_LEVEL2) != 0)
            {
                //Console.Write("L2 ");

            }
            if ((steps & SRS_ADDITIONAL) != 0)
            {
               // Console.Write("Additional ");

            }
            if ((steps & SRS_PRINTS) != 0)
            {
                //Console.Write("Prints ");

            }
            //Console.WriteLine(flags);


            if((steps & SRS_LEVEL1) != 0)
            {
                //int sz = BitConverter.ToInt32(rawPayload, 49);
                //int i1 = BitConverter.ToInt32(rawPayload, 62);
                //int vol = BitConverter.ToInt32(rawPayload, 66);
                //double bid = getPrice(rawPayload, 70);
                //double ask = getPrice(rawPayload, 78);
                //double last = getPrice(rawPayload, 86);
                //double dayhigh = getPrice(rawPayload, 94);
                //for(int i = 30; i < 50; i++)
                //{
                //    Console.Write(rawPayload[i].ToString("X"));
                //}
                //Console.WriteLine();
                
                //Console.WriteLine($"REFL1 Bid {Symbol} {bid} ask {ask} last {last} dayHigh {dayhigh} vol {vol} i1 {i1}");
                //string mzr = ASCIIEncoding.ASCII.GetString(rawPayload, 134, 3);

            }
            int offset = 22;
            if (rawPayload[14] == 15) offset = 207;
            if (rawPayload[14] == 56) offset = 207;
            //for (int ol = 20; ol < 60; ol++)
            int bookTradeCount = (len - offset) / 32;
            List<string> lines = new List<string>();
            lines.Add("------------------------------------------------------");
            for (int i = 0; i < bookTradeCount; i++)
            {
                byte[] ba = new byte[32];
                Array.Copy(rawPayload, (32 * i) + offset, ba, 0, 32);
                Int64 ReferenceNumber = BitConverter.ToInt64(ba, 0);
                Double Price = getPrice(ba, 8);
                int Size = BitConverter.ToInt32(ba, 16);
                string MMID = ASCIIEncoding.ASCII.GetString(ba, 20, 4);
                lines.Add(new StringBuilder().Append(totalPackets).Append(" ").Append(Price).Append(" ").Append(Size).Append(" ").Append(MMID).ToString());
                totalPackets++;
            }
            //File.AppendAllLines(@"e:\foo.txt", lines);
            //}
        }
        //unsigned int m_requestID;
        //Symbol m_symbol;
        //unsigned short m_symbolIndex;
        //unsigned char m_steps;
        //unsigned char m_flags;
        //unsigned char m_bookID;
        //char m_marketStatus;
        public UInt32 RequestID { get { return BitConverter.ToUInt32(rawPayload,2); } set { } }
        public override string Symbol { get { return ASCIIEncoding.ASCII.GetString(rawPayload, 6, 8).TrimEnd((Char)0); }  }
        public UInt16 SymbolIndex { get { return BitConverter.ToUInt16(rawPayload, 14); } set { } }
        public byte Steps { get { return rawPayload[14]; } set { } }
        public byte Flags { get { return rawPayload[17]; } set { } }
        public byte BookID { get { return rawPayload[18]; } set { } }
        public byte MarketStatus { get { return rawPayload[19]; } set { } }
        public int Size { get { return BitConverter.ToInt32(rawPayload, 49); } set { } }
        public int Volume { get { return rawPayload.Length > 66 ? BitConverter.ToInt32(rawPayload, 62): 0; } set { } }
        public Double Bid { get { return getPrice(rawPayload,70); } set { } }
        public Double Ask { get { return getPrice(rawPayload, 78); } set { } }
        public Double P1 { get { return getPrice(rawPayload, 86); } set { } }
        public Double P2 { get { return getPrice(rawPayload, 94); } set { } }
        public string CompanyName { get {
                string rv = null;
                if (rawPayload[14] == 15)
                {
                    rv =  ASCIIEncoding.ASCII.GetString(rawPayload, 18, 20).TrimEnd((char)0);
                }
                return rv;
            } set { } }

        const byte SRS_DESCRIPTION = 1 << 0;
        const byte SRS_ATTRIBUTES = 1 << 1;
        const byte SRS_LEVEL1 = 1 << 2;
        const byte SRS_LEVEL2 = 1 << 3;
        const byte SRS_ADDITIONAL = 1 << 4;
        const byte SRS_PRINTS = 1 << 5;
    public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if(rawPayload[14] == 15) sb.Append(CompanyName).Append(" ").Append(Size).Append(" ")
                .Append(Volume).Append(" ").Append(Bid).Append(" ").Append(Ask).Append(" ")
                .Append(P1).Append(" ").Append(P2).Append(" ");
                sb.Append(RequestID).Append(" ").Append(Symbol)
                .Append(" si=").Append(SymbolIndex).Append(" st=").Append(Steps)
                .Append(" f=").Append(Flags).Append(" bi=").Append(BookID)
                .Append(" mi=").Append(MarketStatus);
                return sb.ToString();
        }

    }
}
