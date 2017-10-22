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

namespace QuoteTester
{
    class config
    {
        public static string directory = @"e:\";
        //public static IPEndPoint quoteServerEP = new IPEndPoint(IPAddress.Parse("10.101.3.206"), 19100);
        public static IPEndPoint quoteServerEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5555);
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
        public static string[] GetSymbols()
        {
            List<string> symbols = new List<string>();
            symbols.AddRange((from s in File.ReadAllLines($@"e:\symbolinfo{DateTime.Now.ToString("yyyyMMdd")}.txt") select s.Split(',')[0]));
            //symbols.AddRange((from s in File.ReadAllLines(@"c:\users\scott\downloads\nasdaq.csv") select s.Split(',')[0]).Skip(1));
            //symbols.AddRange((from s in File.ReadAllLines(@"c:\users\scott\downloads\nyse.csv") select s.Split(',')[0]).Skip(1));
            //symbols.AddRange((from s in File.ReadAllLines(@"c:\users\scott\downloads\amex.csv") select s.Split(',')[0]).Skip(1));
            symbols = (from s in symbols select s.Replace("\"", "")).ToList<string>();
            return symbols.ToArray();
        }
    }
    class Program
    {
        static Dictionary<string, DateTime> outstanding = new Dictionary<string, DateTime>();
        static void Main(string[] args)
        {
            string[] symbols = config.GetSymbols();
            foreach (string s in symbols)
            {
                QuoteHolder qh = new QuoteHolder(s);
                string symbol = s.Split(' ')[0];
                dict[symbol] = qh;
            }
            dict["SPY"] = new QuoteHolder("SPY 1.21 183869 61.75");
            //symbols = (from s in symbols select s).Skip(2500).Take(1000).ToArray();
            //File.WriteAllLines(@"e:\symbs.txt", symbols);
            //Packet.audit = File.OpenWrite(@"e:\audit" + DateTime.Now.ToString("HHmmss") + ".bin");
            TcpClient cl = new TcpClient();
            cl.Connect(config.quoteServerEP);
            //cl.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5555));
            StreamWriter sw = new StreamWriter(cl.GetStream());
            sw.AutoFlush = true;
            Stream str = cl.GetStream();
            byte[] key = { 0x08, 0x00, 0x0eb, 0x03, 0x03, 0x00, 0x00, 0x00 };
            str.Write(key, 0, 8);
            str.Flush();
            //str.Write(connect, 0, connect.Length);
            str.Write(config.logonRequest, 0, config.logonRequest.Length);
            str.Flush();
            Packet.GetPacket(str);
            //str.Write(config.zeroRequest, 0, config.zeroRequest.Length);
            //z = true;
            //Packet.GetPacket(str);
            str.Flush();

            byte[] last = null;
            new Thread(MessageLoop).Start(str);

            new Thread(() => {
                Thread.Sleep(1000);
                lock (outstanding)
                {
                    foreach (string k in outstanding.Keys)
                    {
                        if ((DateTime.Now - outstanding[k]).Seconds > 2)
                        {
                            Console.WriteLine($"remove {k}");
                            outstanding.Remove(k);
                        }
                    }
                }
            }).Start();
            //new Thread(() => {
            //    for (;;)
            //    {
            //        string s = Console.ReadLine();
            //        Console.WriteLine(s);
            //        watching = s;

            //        if (dict.ContainsKey(s))
            //        {
            //            QuoteHolder qh = dict[s];
            //            double bid = qh.Resp.Count > 0 ? qh.Resp[0].Bid : 0;
            //            Console.WriteLine($"bid={bid}");
            //            string l1 = qh.L1 == null ? "XX" : qh.L1.ToString();
            //            Console.WriteLine($"s ={l1}");
            //        }
            //    }
            //}).Start();
            foreach (string s in symbols)
            {
                int c = 0;
                string symbol = s.Split(' ')[0];
                //QuoteHolder qh = new QuoteHolder(s);
                //dict[symbol] = qh;
                //byte[] req = { 0x10, 0x00, 0xbd, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                byte[] req = new byte[config.symbolRequest.Length];
                Array.Copy(config.symbolRequest, req, req.Length);
                Array.Copy(ASCIIEncoding.ASCII.GetBytes(symbol), 0, req, 4, symbol.Length);
                str.Write(req, 0, req.Length);
                //Console.WriteLine("subscribing " + symbol);
                str.Flush();
                lock (outstanding)
                {
                    c = outstanding.Count;
                }
                while (c > 50)
                {
                    Thread.Sleep(100);
                    Console.WriteLine("sleeping");
                }
            }

        }
        private static void unsubscribe(Stream str, string symbol)
        {
            byte[] msg = { 12, 0, 22, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            byte[] sym = ASCIIEncoding.ASCII.GetBytes(symbol);
            Array.Copy(sym, 0, msg, 4, sym.Length);
            str.Write(msg, 0, msg.Length);
            str.Flush();
        }
        public static void sendMessage(string message)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPAddress send_to_address = IPAddress.Parse("127.0.0.1");
            IPEndPoint sending_end_point = new IPEndPoint(send_to_address, 5555);
            socket.SendTo(ASCIIEncoding.ASCII.GetBytes(message), sending_end_point);
        }
        private static Dictionary<string, QuoteHolder> dict = new Dictionary<string, QuoteHolder>();
        static string watching = null;
        static bool T92750 = false;
        static DateTime dtt = DateTime.Now.Date.AddHours(8).AddMinutes(27).AddSeconds(50);
        //static DateTime dtt = DateTime.Now.AddSeconds(30);

        private static void MessageLoop(object o)
        {
            Console.WriteLine(dtt);
            Stream str = (Stream)o;
            sendMessage($"Window will open at {dtt}");
            int packetCount = 0;
            int packetInterval = 0;
            int l1 = 0;
            int resp = 0;
            int trade = 0;
            DateTime dt = DateTime.Now;
            DateTime ldt = DateTime.Now;
            QuoteHolder qh;
            for (;;)
            {
                //Packet p = new Packet(str);
                Packet p = Packet.GetPacket(str);
                if (!T92750 && DateTime.Now > dtt)
                {
                    Console.WriteLine($"window open {DateTime.Now}");
                    T92750 = true;
                    foreach (string sym in dict.Keys)
                    {
                        QuoteHolder qhx = dict[sym];
                        double Ask = -1;
                        if (qhx.L1 == null)
                        {
                            if (qhx.Resp.Count > 0)
                            {
                                if (qhx.Resp[0].rawPayload[14] == 15)
                                {
                                    Ask = qhx.Resp[0].Ask;
                                }
                            }
                        }
                        else
                        {
                            Ask = qhx.L1.Ask;
                        }
                        if (qhx.Volume < 100 && Ask > qhx.PrevClose)
                        {
                            double TargetPrice = qhx.PrevClose - (qhx.ATR * .25);
                            string m = $"B {sym} Ask={Ask} PrevClose={qhx.PrevClose} vol={qhx.Volume} atr={qhx.ATR} buyprice={TargetPrice.ToString("0.00")}";
                            sendMessage(m);
                        }
                    }
                }
                try
                {
                    packetCount++;
                    packetInterval++;
                    if ((DateTime.Now - ldt).TotalSeconds > 10)
                    {
                        Console.WriteLine("total = " + packetCount / 1000000 + " interval = " + packetInterval + " l1=" + l1 + " resp=" + resp + " trade=" + trade + " symbols = " + dict.Keys.Count);
                        l1 = 0; trade = 0; packetInterval = 0;
                        ldt = DateTime.Now;
                        //Packet.audit.Flush();
                    }
                    switch (p.packetType)
                    {
                        case Packet.PacketType.M_RESP_REFRESH_SYMBOL:
                            //Console.WriteLine("resp");
                            resp++;
                            RespRefreshSymbolPacket rp = (RespRefreshSymbolPacket)p;
                            lock (outstanding)
                            {
                                if (outstanding.ContainsKey(rp.Symbol))
                                {
                                    outstanding.Remove(rp.Symbol);
                                }
                            }
                            if (dict.ContainsKey(rp.Symbol))
                            {
                                qh = dict[rp.Symbol];
                                qh.Resp.Add(rp);
                                qh.Volume = qh.Resp[0].Volume;
                            }
                            break;
                        case Packet.PacketType.M_TRADE_REPORT:
                            trade++;
                            TradeReportPacket tp = (TradeReportPacket)p;
                            string ts = tp.Symbol;
                            qh = dict[ts];
                            qh.Volume += tp.Size;
                            //Console.WriteLine(p);
                            if (watching == ts)
                            {
                                Console.WriteLine("TR + " + qh.L1 == null ? "xx" : qh.L1 + " " + qh.Volume + " " + tp.Size);
                            }
                            break;
                        case Packet.PacketType.M_BOOK_TRADE:
                            //trade++;                       
                            //Console.WriteLine(p);
                            break;
                        case Packet.PacketType.M_LEVEL1:
                            l1++;
                            L1Packet q = (L1Packet)p;
                            string qs = q.Symbol;
                            try
                            {
                                qh = dict[qs];
                                //double pct = .2;
                                //double targetPrice = qh.PrevClose + (qh.ATR * pct);
                                ////if (watching == qs)
                                //if (!qh.hasPosition)
                                //{
                                //    if (q.Bid > targetPrice)
                                //    {
                                //        string s = $"{qs} prevClose={qh.PrevClose} targetPrice={targetPrice}  atr={qh.ATR} bid={q.Bid} pct={pct.ToString("0.00")}";
                                //        Console.WriteLine(s);
                                //        qh.hasPosition = true;
                                //        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                                //        IPAddress send_to_address = IPAddress.Parse("127.0.0.1");
                                //        IPEndPoint sending_end_point = new IPEndPoint(send_to_address, 5555);
                                //        socket.SendTo(ASCIIEncoding.ASCII.GetBytes(s), sending_end_point);
                                //    }
                                //    //Console.WriteLine($"{q} {qh.Volume} {qh.PrevClose} {qh.ATR}");
                                //}
                                qh.L1 = (L1Packet)p;
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("unsolicited pack");
                            }
                            break;
                        case Packet.PacketType.M_BOOK_NEW_QUOTE:
                        case Packet.PacketType.M_BOOK_MODIFY_QUOTE:
                        case Packet.PacketType.M_BOOK_DELETE_QUOTE:
                        case Packet.PacketType.M_STOCK_IMBALANCE_INDICATOR:
                        case Packet.PacketType.M_BOOK_ORDER_CANCELED:
                            break;
                        default:
                            //Console.WriteLine(p);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);

                }
            }
        }
    }
}
