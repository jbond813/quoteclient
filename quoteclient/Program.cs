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

namespace quoteclient
{
    static class config
    {
        public static string directory;// = @"d:\";
        //public static string directory = @"e:\";
        public static IPEndPoint quoteServerEP = new IPEndPoint(IPAddress.Parse("10.101.3.206"), 19100);
        //public static IPEndPoint quoteServerEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5555);
        public static byte[] getlogonRequest() {
            return File.ReadAllBytes(directory + "rawconnect.dat");
        }
        //public static IPEndPoint quoteServerEP = new IPEndPoint(IPAddress.Parse("10.101.3.217"), 7400);
        public static byte[] getzeroRequest() {
            return File.ReadAllBytes(directory + "raw0refresh.dat");
        }
        public static byte[] getsymbolRequest() {
            byte[] b = File.ReadAllBytes(directory + "rawpgrefresh.dat");
            b[4] = 0x00;
            b[5] = 0x00;
            return b;
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
            if (args.Length > 0) config.directory = $"{args[0]}:\\";
            else config.directory = "d:\\";
            StatusListener l = new StatusListener(dict);
            string[] symbols = config.GetSymbols();
            //symbols = (from s in symbols select s).Skip(2500).Take(1000).ToArray();
            File.WriteAllLines($@"{config.directory}fff\symbs.txt",symbols);
            Packet.audit = File.OpenWrite($@"{config.directory}fff\audit" + DateTime.Now.ToString("HHmmss") + ".bin");
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
            str.Write(config.getlogonRequest(), 0, config.getlogonRequest().Length);
            str.Flush();
            Packet.GetPacket(str);
            //str.Write(config.zeroRequest, 0, config.zeroRequest.Length);
            //z = true;
            //Packet.GetPacket(str);
            str.Flush();

            //byte[] last = null;
            InitSocket();
            foreach(string sym in symbols)
            {
                string sl = "SYMBOLLIST";
                sl += $"|{sym.Split(' ')[0]}";
                sendMessage(sl);
            }
            new Thread(MessageLoop).Start(str);
            new Thread(ReceiveServerMessages).Start();
            dict["BG"] = new QuoteHolder("BG 1.21 183869 61.75");
            foreach (string s in symbols)
            {
                int c = 0;
                QuoteHolder qh = new QuoteHolder(s);
                string symbol = s.Split(' ')[0];
                dict[symbol] = qh;
                //byte[] req = { 0x10, 0x00, 0xbd, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                byte[] req = new byte[config.getsymbolRequest().Length];
                Array.Copy(config.getsymbolRequest(), req, req.Length);
                Array.Copy(ASCIIEncoding.ASCII.GetBytes(symbol), 0, req, 4, symbol.Length);
                str.Write(req, 0, req.Length);
                //Console.WriteLine("subscribing " + symbol);
                str.Flush();
                lock (outstanding) {
                    c = outstanding.Count;
                }
                while (c > 50)
                {
                    Thread.Sleep(100);
                    Console.WriteLine("sleeping");
                }
            }
            Console.WriteLine("all symbols sent");
        }
        private static void unsubscribe(Stream str, string symbol)
        {
            byte[] msg = { 12, 0, 22, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            byte[] sym = ASCIIEncoding.ASCII.GetBytes(symbol);
            Array.Copy(sym, 0, msg, 4, sym.Length);
            str.Write(msg, 0, msg.Length);
            str.Flush();
        }
        static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        static void ReceiveServerMessages()
        {
            byte[] buff = new byte[1000];
            EndPoint rep = new IPEndPoint(IPAddress.Any, 0);
            int n;
            for (;;)
            {
                n = socket.ReceiveFrom(buff, ref rep);
                string message = ASCIIEncoding.ASCII.GetString(buff, 0, n);
                if(message.StartsWith("POS "))
                {
                    string[] toks = message.Split(' ');
                    ExistingPosition.Add(toks[1], Int32.Parse(toks[2]));
                }
                if (!message.StartsWith("message type="))
                {
                    Console.WriteLine(message);
                }
                if (message.StartsWith("SENTORDER"))
                {
                    string[] toks = message.Split('|');
                    string symbol = toks[1];
                    int myid = Int32.Parse(toks[2]);
                    int theirid = Int32.Parse(toks[3]);
                    if (dict.ContainsKey(symbol)) {
                        QuoteHolder qh = dict[symbol];
                        Console.WriteLine($"SO moid{qh.OpeningOrderMyID} mLid{qh.LimitOrderMyID} mid{myid} thid{theirid}");
                        if (qh.LimitOrderMyID == myid)
                        {
                            qh.LimitOrderID = theirid;
                        }
                        if (qh.OpeningOrderMyID == myid)
                        {
                            qh.OpeningOrderID = theirid;
                        }
                        if (qh.StopOrderMyID == myid)
                        {
                            qh.StopOrderID = theirid;
                        }
                    }
                    if (OrderMap.ContainsKey(myid))
                    {
                        OrderMap[myid] = theirid;
                    }
                }
                //if (message.StartsWith("NEWORDER"))
                //{
                //    //string[] toks = message.Split('|');
                //    //string symbol = toks[1];
                //    //int myid = Int32.Parse(toks[2]);
                //    //int theirid = Int32.Parse(toks[3]);
                //    if (dict.ContainsKey(symbol))
                //    {
                //        QuoteHolder qh = dict[symbol];
                //        if (qh.MyID == myid)
                //        {
                //            qh.LimitOrderID = theirid;
                //        }
                //    }
                //    if (OrderMap.ContainsKey(myid))
                //    {
                //        OrderMap[myid] = theirid;
                //    }
                //}
                if (message.StartsWith("DEAD"))
                {
                    string[] toks = message.Split('|');
                    string symbol = toks[1];
                    int clientID = Int32.Parse(toks[2]);
                    char side = toks[3][0];
                    int quantity = Int32.Parse(toks[4]);
                    int openQuantity = Int32.Parse(toks[5]);
                    Console.WriteLine("Dead Message");
                    if (dict.ContainsKey(symbol))
                    {
                        QuoteHolder qh = dict[symbol];
                        Console.WriteLine($"{qh.OpeningOrderID}");
                        if (qh.OpeningOrderID == clientID)
                        {
                            qh.DeadOpen = true;
                            int sharesToClose = quantity - openQuantity;
                            char closeSide = side == 'B' ? 'S' : 'B';
                            double TargetPrice = qh.PrevClose - .02; //testing
                            Snapshot snap = qh.Resp[0].Snapshot;
                            Double Bid = qh.L1 == null ? snap.Bid : qh.L1.Bid;
                            Double Ask = qh.L1 == null ? snap.Ask : qh.L1.Ask;
                            //string m = $"{closeSide}|{symbol}|{0}|{0}|0|{TargetPrice.ToString("0.00")}|0|0|DAY|{OPGOrderID}|{sharesToClose}"; //TESTING = shour be OPG
                            string m = $"ORDER|S|{symbol}|{TargetPrice.ToString("0.00")}|{Bid.ToString("0.00")}|{Ask.ToString("0.00")}|DAY|{OPGOrderID}|{sharesToClose}";

                            qh.LimitOrderMyID = OPGOrderID;
                            qh.LimitPrice = TargetPrice;
                            OrderMap[OPGOrderID] = 0;
                            OPGOrderID++;
                            //qhx.PostExec(sym, 100, TargetPrice);
                            Console.WriteLine(m);
                            sendMessage(m);
                        }
                        if (qh.LimitOrderID == clientID)
                        {
                            qh.LimitOut = true;
                            qh.DeadLimit = true;
                            if(qh.OpenShares > 0 && ! qh.stopRequested)
                            {
                                string m = SendStop(symbol);
                                qh.stopRequested = true;
                                sendMessage(m);
                            }
                            //int sharesToClose = quantity - openQuantity                        
                        }
                        if (qh.StopOrderID == clientID)
                        {
                            qh.DeadStop = true;
                        }
                    }
                }
                if (message.StartsWith("TM_NEW_EXECUTION"))
                {
                    string[] toks = message.Split('|');
                    string symbol = toks[1];
                    double price = double.Parse(toks[2]);
                    int execQty = Int32.Parse(toks[3]);
                    int posQty = Int32.Parse(toks[4]);
                    char side = toks[5][0];
                    int clientOrderID =Int32.Parse(toks[6]);
                    if (dict.ContainsKey(symbol))
                    {
                        QuoteHolder qh = dict[symbol];
                        if(qh.OpeningOrderID == clientOrderID)
                        {
                            qh.PostExec(symbol, execQty, price);
                        }
                        //if (OrderMap.Values.Contains(clientOrderID))
                        //{
                        //    qh.PostExec(symbol, execQty, price);
                        //}
                        double TargetPrice = price + qh.ATR;
                        //if (qh.LimitPrice == 0) //this prevents double closing order
                        //{
                        //    string m = $"S|{symbol}|{0}|{0}|0|{TargetPrice.ToString("0.00")}|0|0|DAY|{OPGOrderID}|{execQty}"; //TESTING = shour be OPG
                        //    qh.LimitOrderMyID = OPGOrderID;
                        //    qh.LimitPrice = TargetPrice;
                        //    OrderMap[OPGOrderID] = 0;
                        //    OPGOrderID++;
                        //    //qhx.PostExec(sym, 100, TargetPrice);
                        //    Console.WriteLine(m);
                        //    sendMessage(m);
                        //}
                    }
                }
            }
        }
        public static void InitSocket()
        {
            IPAddress send_to_address = IPAddress.Parse("127.0.0.1");
            IPEndPoint sending_end_point = new IPEndPoint(send_to_address, 5555);
            socket.SendTo(ASCIIEncoding.ASCII.GetBytes("INIT"), sending_end_point);
        }
        public static void sendMessage(string message)
        {
            //Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPAddress send_to_address = IPAddress.Parse("127.0.0.1");
            IPEndPoint sending_end_point = new IPEndPoint(send_to_address, 5555);
            socket.SendTo(ASCIIEncoding.ASCII.GetBytes(message), sending_end_point);
        }
        //private static Dictionary<string, L1Packet> L1Dict = new Dictionary<string, L1Packet>();
        //private static Dictionary<string, List<RespRefreshSymbolPacket>> refDict = new Dictionary<string, List<RespRefreshSymbolPacket>>();
        //private static Dictionary<string, int> volume = new Dictionary<string, int>();
        private static ConcurrentDictionary<string, QuoteHolder> dict = new ConcurrentDictionary<string, QuoteHolder>();
        static string watching = null;
        static bool T92750 = false;
        static DateTime dtt = DateTime.Now.Date.AddHours(8).AddMinutes(27).AddSeconds(50);
        //static DateTime dtt = DateTime.Now.AddSeconds(20); //TESTING USE ABOVE LINE

        private static void MessageLoop(object o)
        {
            Console.WriteLine(dtt);
            Stream str = (Stream)o;
            //sendMessage($"Window will open at {dtt}");
            Console.WriteLine($"Window will open at {dtt}");
            int packetCount = 0;
            int packetInterval = 0;
            int l1 = 0;
            int resp = 0;
            int trade = 0;
            DateTime dt = DateTime.Now;
            DateTime ldt = DateTime.Now;
            QuoteHolder qh;
            QuoteHolder.Logger = new StreamWriter($@"{config.directory}fff\Q{DateTime.Now.ToString("HHmmss")}.txt");
            //new Thread(t => {
            //    Thread.Sleep(1000);
            //    QuoteHolder.Logger.Flush();
            //}).Start();
            for (int i = 0 ;;i++)
            {
                //Packet p = new Packet(str);
                Packet p = Packet.GetPacket(str);
                if(!T92750 && DateTime.Now > dtt)
                {
                    ExecuteOPG();
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
                        Packet.audit.Flush();
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
                                //qh.Volume = qh.Resp[0].Volume;
                                const byte SRS_LEVEL1 = 1 << 2;
                                if ((rp.Steps & SRS_LEVEL1) != 0)
                                {
                                    qh.Volume = rp.Snapshot.Volume;
                                    //Console.WriteLine(rp.Snapshot.ToString());

                                }

                            }

                            break;
                        case Packet.PacketType.M_TRADE_REPORT:
                            trade++;
                            TradeReportPacket tp = (TradeReportPacket)p;
                            string ts = tp.Symbol;
                            qh = dict[ts];
                            int QtyToSell = qh.ProcessTrade(tp);
                            //Console.WriteLine($"{tp.Symbol} process Trade returned {QtyToSell}");
                            if(QtyToSell > 0 && !qh.Closing && !qh.LimitOut && !ExistingPosition.ContainsKey(ts) && qh.DeadOpen)
                            {
                                //string m = SendStop(ts);
                                string m = $"CANCEL|{ts}|{qh.LimitOrderID}";
                                //qhx.PostExec(sym, 100, TargetPrice);
                                Console.WriteLine(m);
                                qh.Closing = true;
                                sendMessage(m);

                            }
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
                                qh.L1 = (L1Packet)p;
                            }
                            catch (Exception)
                            {
                                Console.WriteLine($"unsolicitied pack {qs}");
                            }
                            break;
                        case Packet.PacketType.M_BOOK_NEW_QUOTE:
                        case Packet.PacketType.M_BOOK_MODIFY_QUOTE:
                        case Packet.PacketType.M_BOOK_DELETE_QUOTE:
                        case Packet.PacketType.M_STOCK_IMBALANCE_INDICATOR:
                        case Packet.PacketType.M_BOOK_ORDER_CANCELED:
                            //Console.WriteLine("Order Cancelled");
                            break;
                        default:
                            //Console.WriteLine(p);
                            break;
                    }
                } catch(Exception e)
                {
                    Console.WriteLine(e.Message);

                }
            }
        }

        private static string SendStop(string symbol)
        {
         
            QuoteHolder qhx = dict[symbol];
            qhx.stopRequested = true;
            int QtyToSell = qhx.OpenShares;
            Snapshot snap = qhx.Resp[0].Snapshot; //DANGEROUS
                                                                          //double TargetPrice = qhx.Resp[0].Snapshot.Close - (qhx.ATR * .25);
            double Ask = qhx.L1 != null ? qhx.L1.Ask : snap.Ask;
            double Bid = qhx.L1 != null ? qhx.L1.Bid : snap.Bid;
            double TargetPrice = Bid - .35;
                                            //string m = $"S|{tp.Symbol}|{snap.Close}|{qhx.Volume}|{qhx.ATR.ToString("0.00")}|{TargetPrice.ToString("0.00")}|{Bid.ToString("0.00")}|{Ask.ToString("0.00")}|DAY"; //TESTING = shour be OPG
                                            //string m = $"S|{tp.Symbol}|{snap.Close}|{qhx.Volume}|{qhx.ATR.ToString("0.00")}|{TargetPrice.ToString("0.00")}|{Bid.ToString("0.00")}|{Ask.ToString("0.00")}|DAY|{OPGOrderID}"; //TESTING = shour be OPG
//            string m = $"STOP|{symbol}|{TargetPrice.ToString("0.00")}|{Bid.ToString("0.00")}|{Ask.ToString("0.00")}|DAY|{qhx.LimitOrderID}|{QtyToSell}|S|{qhx.OpeningOrderID}"; //TESTING = shour be OPG
            string m = $"ORDER|S|{symbol}|{TargetPrice.ToString("0.00")}|{Bid.ToString("0.00")}|{Ask.ToString("0.00")}|DAY|{OPGOrderID}|{qhx.OpenShares}"; //TESTING = shour be OPG

            Console.WriteLine($"STOP {symbol} stopped out {qhx.Stop} stopPrice {qhx.Stop}");
            qhx.StopOrderMyID = OPGOrderID;
            OrderMap[OPGOrderID] = 0;
            OPGOrderID++;
            return m;
        }

        private static ConcurrentDictionary<int,int> OrderMap = new ConcurrentDictionary<int, int>();
        private static HashSet<int> Limit = new HashSet<int>();
        private static int OPGOrderID = 20000000 + (int)(DateTime.Now.Ticks % 9999);
        private static void ExecuteOPG()
        {
            int countSent = 0;

            Console.WriteLine($"window open {DateTime.Now}");
            T92750 = true;

            foreach (string sym in dict.Keys)
            {
                QuoteHolder qhx = dict[sym];
                double Bid = 0;
                double Ask = 0;
                if (qhx.Resp.Count > 0 && qhx.Resp[0].Snapshot != null)
                {
                    Snapshot snap = qhx.Resp[0].Snapshot;
                    if (qhx.L1 == null)
                    {
                        Bid = snap.Bid;
                        Ask = snap.Ask;
                    }
                    else
                    {
                        Bid = qhx.L1.Bid;
                        Ask = qhx.L1.Ask;
                    }
                    if (qhx.Volume < 100 && Ask > snap.Close)
                    //if (true && !ExistingPosition.ContainsKey(sym)) //TESTING USE ABOVE LINE
                    {
                        if (countSent < 1000)
                        {
                            double TargetPrice = snap.Close - (qhx.ATR * 1.1); //should be .25
                            //char side = toks[1].c_str()[0];
                            //const char* sym = toks[2].c_str();
                            //double price = atof(toks[4].c_str());
                            //double bid = atof(toks[5].c_str());
                            //double ask = atof(toks[6].c_str());
                            //unsigned char TIF = TIF_OPG;
                            //if (!strcmp(toks[7].c_str(), "DAY")) TIF = TIF_DAY;
                            //int myid = atol(toks[8].c_str());
                            //int qty = atol(toks[9].c_str());
                            //TargetPrice = Bid + .01; //TESTING DELETE THIS LINE
                            //string m = $"ORDER|B|{sym}|{TargetPrice.ToString("0.00")}|{Bid.ToString("0.00")}|{Ask.ToString("0.00")}|OPG|{OPGOrderID}|100";
                            //string m = $"ORDER|B|{sym}|{TargetPrice.ToString("0.00")}|{Bid.ToString("0.00")}|{Ask.ToString("0.00")}|5|{OPGOrderID}|1"; //TESTING = shour be OPG
                            string m = $"ORDER|B|{sym}|{TargetPrice.ToString("0.00")}|{Bid.ToString("0.00")}|{Ask.ToString("0.00")}|OPG|{OPGOrderID}|1"; //PROD
                            //string m = $"ORDER|B|{sym}|{TargetPrice.ToString("0.00")}|{Bid.ToString("0.00")}|{Ask.ToString("0.00")}|DAY|{OPGOrderID}|100"; //TESTING = shour be OPG
                            qhx.OpeningOrderMyID = OPGOrderID;
                            OrderMap[OPGOrderID] = 0;
                            OPGOrderID++;
                            //qhx.PostExec(sym, 100, TargetPrice);
                            Console.WriteLine(m);
                            sendMessage(m);
                            countSent++;
                        }

                    }

                }
            }
            Console.WriteLine($"window closed {DateTime.Now}");
        }
    }
}
