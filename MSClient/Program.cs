using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace MSClient
{
    class config
    {
        public static string directory = @"e:\";
        public static IPEndPoint quoteServerEP = new IPEndPoint(IPAddress.Parse("10.101.3.206"), 19100);
        //public static IPEndPoint quoteServerEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5555);
        public static byte[] logonRequest = File.ReadAllBytes(directory + "rawconnect.dat");
        //public static IPEndPoint quoteServerEP = new IPEndPoint(IPAddress.Parse("10.101.3.217"), 7400);
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
        static void Main(string[] args)
        {
            //string[] symbols = config.GetSymbols();
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
            Packet.GetPacket(str);
            str.Write(config.zeroRequest, 0, config.zeroRequest.Length);
            //z = true;
            //Packet.GetPacket(str);
            str.Flush();

            byte[] last = null;
            new Thread(MessageLoop).Start(str);

            //for (;;)
            //{
            //    string symbol = Console.ReadLine();
            //    if (!symbol.EndsWith("-"))
            //    {
            //        byte[] req = { 0x10, 0x00, 0xbd, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            //        Array.Copy(ASCIIEncoding.ASCII.GetBytes(symbol), 0, req, 4, symbol.Length);
            //        //config.updateSymbol(symbol);
            //        str.Write(req, 0, req.Length);
            //        str.Flush();
            //    }
            //    else
            //    {
            //        symbol = symbol.TrimEnd('-');
            //        unsubscribe(str, symbol);
            //        Console.WriteLine("sent unsubscribe " + symbol);
            //    }
            //}
        }
        private static void unsubscribe(Stream str, string symbol)
        {
            byte[] msg = { 12, 0, 22, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            byte[] sym = ASCIIEncoding.ASCII.GetBytes(symbol);
            Array.Copy(sym, 0, msg, 4, sym.Length);
            str.Write(msg, 0, msg.Length);
            str.Flush();
        }

        private static void MessageLoop(object o)
        {
            //}
        }
    }
}
