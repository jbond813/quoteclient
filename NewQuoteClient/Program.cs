using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using MDPackets;
using System.Collections.Concurrent;
using MarketData;
using Modules;

namespace NewQuoteClient
{
    static class config
    {
        public static string directory;// = @"d:\";
        //public static string directory = @"e:\";
        public static IPEndPoint quoteServerEP = new IPEndPoint(IPAddress.Parse("10.101.3.206"), 19100);
        //public static IPEndPoint quoteServerEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5555);
        public static byte[] getlogonRequest()
        {
            return File.ReadAllBytes(directory + "rawconnect.dat");
        }
        //public static IPEndPoint quoteServerEP = new IPEndPoint(IPAddress.Parse("10.101.3.217"), 7400);
        public static byte[] getzeroRequest()
        {
            return File.ReadAllBytes(directory + "raw0refresh.dat");
        }
        public static byte[] getsymbolRequest()
        {
            return File.ReadAllBytes(directory + "rawpgrefresh.dat");
        }
        public static void updateSymbol(string symbol)
        {
            byte[] sym = ASCIIEncoding.ASCII.GetBytes(symbol);
            Array.Copy(sym, 0, getsymbolRequest(), 4, sym.Length);
            getsymbolRequest()[sym.Length + 4] = 0;
        }
        public static string[] GetSymbols()
        {
            List<string> symbols = new List<string>();
            symbols.AddRange((from s in File.ReadAllLines($@"{config.directory}symbolinfo{DateTime.Now.ToString("yyyyMMdd")}.txt") select s.Split(',')[0]));
            //symbols.AddRange((from s in File.ReadAllLines(@"c:\users\scott\downloads\nasdaq.csv") select s.Split(',')[0]).Skip(1));
            //symbols.AddRange((from s in File.ReadAllLines(@"c:\users\scott\downloads\nyse.csv") select s.Split(',')[0]).Skip(1));
            //symbols.AddRange((from s in File.ReadAllLines(@"c:\users\scott\downloads\amex.csv") select s.Split(',')[0]).Skip(1));
            symbols = (from s in symbols select s.Replace("\"", "")).ToList<string>();
            return symbols.ToArray();
        }
    }
    class Program
    {
        static Dictionary<string, int> ExistingPosition = new Dictionary<string, int>();
        static Dictionary<string, DateTime> outstanding = new Dictionary<string, DateTime>();
        static void Main(string[] args)
        {
            //BlockingCollection<Packet> queue = new BlockingCollection<Packet>();
            StrategyRunner sr = new StrategyRunner();
            sr.Run();
            for(;;)
            {
                Packet p;
                p = sr.queue.Take();
                switch(p.packetType)
                {
                    case Packet.PacketType.ORDER_SENT:
                        OrderSent os = (OrderSent)p;
                        Console.WriteLine(os);
                        break;
                }
                Console.WriteLine(p);
            }
        }
    }
}
