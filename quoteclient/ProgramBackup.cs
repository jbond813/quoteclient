using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace quoteclient
{
    class config
    {
        public static string directory = @"e:\";
        //public static IPEndPoint quoteServerEP = new IPEndPoint(IPAddress.Parse("10.101.3.206"), 19100);
        public static IPEndPoint quoteServerEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5555);
        public static byte[] logonRequest = File.ReadAllBytes(directory + "rawconnect.dat");
        public static byte[] zeroRequest = File.ReadAllBytes(directory + "raw0refresh.dat");
        public static byte[] symbolRequest = File.ReadAllBytes(directory + "rawpgrefresh.dat");
        public static void updateSymbol(string symbol)
        {
            byte[] sym = ASCIIEncoding.ASCII.GetBytes(symbol);
            Array.Copy(sym, 0, symbolRequest, 4, sym.Length);
            symbolRequest[sym.Length + 4] = 0;
        }
    }
    class Program
    {
        //static string getTime(byte[] b, int off)
        //{
        //    int t = BitConverter.ToInt32(b, off);
        //    int tmp;
        //    tmp = t;
        //    int h = tmp / 3600000;
        //    tmp -= (h * 3600000);
        //    int m = tmp / 60000;
        //    tmp -= (m * 60000);
        //    int s = tmp / 1000;
        //    int f = tmp % 1000;
        //    string rv = h.ToString("00:") + m.ToString("00:") + s.ToString("00") + "." + f.ToString("000");
        //    return rv;
        //}
        //static double getPrice(byte[] b, int off)
        //{
        //    double rv;
        //    int d = BitConverter.ToInt32(b, off);
        //    double  c = (double)BitConverter.ToInt32(b, off + 4);
        //    rv = d + (c / 1000000000);
        //    return rv;
        //}
        static void Main(string[] args)
        {
            //byte[] znext = File.ReadAllBytes(@"c:\src\znext.raw");
            //for(int i = 26;i < znext.Length - 4;i+=24)
            //{
            //    string t = getTime(znext,i + 8);
            //    double price = getPrice(znext, i);
            //    int j = BitConverter.ToInt32(znext, i + 12);
            //    string s1 = i.ToString("X4") + " " + j;
            //    j = BitConverter.ToInt32(znext, i + 16);
            //    //string s2 = i.ToString("X4") + " " + j;
            //    string s2a = ASCIIEncoding.ASCII.GetString(znext, i + 16, 4);
            //    j = BitConverter.ToInt32(znext, i + 20);
            //    string s3 = i.ToString("X4") + " " + j;
            //}
            //byte[] connect = File.ReadAllBytes(@"e:\rawconnect.dat");
            //byte[] pgrefresh = File.ReadAllBytes(@"e:\rawpgrefresh.dat");
            //pgrefresh[4] = 83;
            //pgrefresh[5] = 80;
            //pgrefresh[6] = 89;
            //pgrefresh[7] = 0;
            //byte[] zerorefresh = File.ReadAllBytes(@"e:\raw0refresh.dat");
            TcpClient cl = new TcpClient();
            cl.Connect(config.quoteServerEP);
            //cl.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5555));
            StreamWriter sw = new StreamWriter(cl.GetStream());
            sw.AutoFlush = true;
            Stream str = cl.GetStream();
            byte[] key = { 0x08, 0x00, 0x0eb, 0x03, 0x00, 0x00, 0x00, 0x00 };
            str.Write(key, 0, 8);
            str.Flush();
            //str.Write(connect, 0, connect.Length);
            str.Write(config.logonRequest, 0, config.logonRequest.Length);
            str.Flush();
            byte[] r = new byte[100000];
            bool z = false;
            bool done = false;
            //new Thread(t =>
            //{
                for (;;)
                {
                    //Packet p = new Packet(str);
                    Packet p = Packet.GetPacket(str);
                    if (p.packetType == Packet.PacketType.M_LEVEL1)
                    {
                        L1Packet l1 = (L1Packet)(p);
                        //l1.ToString();
                        Console.WriteLine(l1);
                    }
                    //byte[] lbuff = new byte[2];
                    //if (!partialPacket)
                    //{
                    //    str.Read(lbuff, 0, 2);
                    //    length = BitConverter.ToUInt16(lbuff, 0) - 2;
                    //    offset = 0;
                    //}
                    //Console.Write("packet length " + length);
                    //int count = str.Read(r, offset, length);
                    //packetType = BitConverter.ToUInt16(r, 0);
                    //Console.WriteLine(" read " + count + " bytes packettype " + packetType);
                    //if(count != length)
                    //{
                    //    partialPacket = true;
                    //    length -= count;
                    //    offset = count;
                    //}
                    //else
                    //{
                    //    partialPacket = false;
                    //}
                    if (!z)
                    {
                        str.Write(config.zeroRequest, 0, config.zeroRequest.Length);
                        z = true;
                        str.Flush();
                    }
                    else
                    {
                        if(!done)
                        {
                            config.updateSymbol("SPY");
                            str.Write(config.symbolRequest, 0, config.symbolRequest.Length);
                            done = true;
                            str.Flush();
                        }
                    }
                }
            //}).Start();
        }
    }
}
