using MDPackets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TestDataGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient cl = new TcpClient();
            cl.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5555));
            for(;;)
            {
                Packet p = Packet.GetPacket(cl.GetStream());
                if(p.packetType == Packet.PacketType.M_RESP_REFRESH_SYMBOL)
                {
                    char[] s = new char[p.rawPayload.Length * 2];
                    for(int i = 0;i < p.rawPayload.Length;i++)
                    {
                        string st = p.rawPayload[i].ToString("X2");
                        Array.Copy(st.ToArray(), 0, s, i * 2, 2);
                    }
                    string str = new string(s);
                    string strf = $"string str = \"{str}\";";
                    //File.WriteAllText(@"e:\RESPREF.txt",strf);
                }
                if (p.packetType == Packet.PacketType.M_LEVEL1)
                {
                    char[] s = new char[p.rawPayload.Length * 2];
                    for (int i = 0; i < p.rawPayload.Length; i++)
                    {
                        string st = p.rawPayload[i].ToString("X2");
                        Array.Copy(st.ToArray(), 0, s, i * 2, 2);
                    }
                    string str = new string(s);
                    string strf = $"string str = \"{str}\";";
                    //File.WriteAllText(@"e:\L1.txt", strf);
                    //return;
                }
                if (p.packetType == Packet.PacketType.M_TRADE_REPORT)
                {
                    char[] s = new char[p.rawPayload.Length * 2];
                    for (int i = 0; i < p.rawPayload.Length; i++)
                    {
                        string st = p.rawPayload[i].ToString("X2");
                        Array.Copy(st.ToArray(), 0, s, i * 2, 2);
                    }
                    string str = new string(s);
                    string strf = $"string str = \"{str}\";";
                    File.WriteAllText(@"e:\TradeReport.txt", strf);
                    return;
                }
            }
        }
    }
}
