using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ExtensionTester
{
    class Program
    {
        static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        static void Main(string[] args)
        {
            byte[] buff = new byte[1000];
            EndPoint rep = new IPEndPoint(IPAddress.Any, 0);
            int n;
            IPAddress send_to_address = IPAddress.Parse("127.0.0.1");
            IPEndPoint sending_end_point = new IPEndPoint(send_to_address, 5555);
            //char side = toks[1].c_str()[0];
            //const char* sym = toks[1].c_str();
            //double price = atof(toks[4].c_str());
            //double bid = atof(toks[5].c_str());
            //double ask = atof(toks[6].c_str());
            //int myid = atol(toks[8].c_str());
            //int qty = atol(toks[9].c_str());
            //unsigned char TIF = TIF_OPG;
            //if (!strcmp(toks[7].c_str(), "DAY")) TIF = TIF_DAY;
            string message = $"SYMBOLLIST|F|MDB|AAPL|FB|IBM|SCHW|CSCO";
            socket.SendTo(ASCIIEncoding.ASCII.GetBytes(message), sending_end_point);
            int orderId = 5555;
            message = $"ORDER|B|IBM|32.76 |32.70|33.80|OPG|20007716|2";
            socket.SendTo(ASCIIEncoding.ASCII.GetBytes(message), sending_end_point);
            message = $"ORDER|B|IBM|32.76 |32.70|33.80|OPG|20007716|1";
            socket.SendTo(ASCIIEncoding.ASCII.GetBytes(message), sending_end_point);
            message = $"ORDER|B|AAPL|32.76 |32.70|33.80|OPG|20007716|1";
            socket.SendTo(ASCIIEncoding.ASCII.GetBytes(message), sending_end_point);
            message = $"ORDER|S|F|32.76 |32.70|33.80|OPG|20007716|1";
            socket.SendTo(ASCIIEncoding.ASCII.GetBytes(message), sending_end_point);
            n = socket.ReceiveFrom(buff, ref rep);
            message = ASCIIEncoding.ASCII.GetString(buff, 0, n);
            string[] toks = message.Split('|');
            int orderID = Int32.Parse(toks[3]);
            message = $"CANCEL|AAPL|{orderID}";
            socket.SendTo(ASCIIEncoding.ASCII.GetBytes(message), sending_end_point);
            do
            {
                n = socket.ReceiveFrom(buff, ref rep);
                message = ASCIIEncoding.ASCII.GetString(buff, 0, n);
            } while (!message.StartsWith("DEAD"));
            message = $"POSITIONS|5555";
            socket.SendTo(ASCIIEncoding.ASCII.GetBytes(message), sending_end_point);
            do
            {
                n = socket.ReceiveFrom(buff, ref rep);
                message = ASCIIEncoding.ASCII.GetString(buff, 0, n);
            } while (!message.StartsWith("POSDONE"));
        }
    }
}
