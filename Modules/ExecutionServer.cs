using MDPackets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Modules
{
    public class ExecutionServer : IExecutionServer
    {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPEndPoint sending_end_point = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5555);
        BlockingCollection<Packet> Queue;
        int orderID = 20000;
        public int SendOrder(char side, string symbol,double LimitPrice, double Bid, double Ask, string TIF, int qty)
        {
            int ord = orderID++;
            string m = $"ORDER|{side}|{symbol}|{LimitPrice.ToString("0.00")}|{Bid.ToString("0.00")}|{Ask.ToString("0.00")}|{TIF}|{ord}|{qty}"; //PROD
            socket.SendTo(ASCIIEncoding.ASCII.GetBytes(m), sending_end_point);
            return ord;
        }

        public Order SendOrder(OrderCondition cond, Stock s)
        {
            int ord = orderID++;
            string m = $"ORDER|{cond.Side}|{s.Symbol}|{cond.Limit.ToString("0.00")}|{s.Bid.ToString("0.00")}|{s.Ask.ToString("0.00")}|{cond.Tif}|{ord}|{cond.Shares}"; //PROD
            socket.SendTo(ASCIIEncoding.ASCII.GetBytes(m), sending_end_point);
            Order o = new Order() { Condition = cond, LimitPrice = cond.Limit, MyID = ord, Symbol = s.Symbol };
            return o;
            //throw new NotImplementedException();
        }

        public void CancelOrder(Order o)
        {
            throw new NotImplementedException();
        }

        public ExecutionServer(BlockingCollection<Packet> queue)
        {
            Queue = queue;
            socket.SendTo(ASCIIEncoding.ASCII.GetBytes("INIT"), sending_end_point);
            new Thread(t => {
                for (;;)
                {
                    Packet p = Packet.GetPacket(socket);
                    if(p != null) queue.Add(p);
                }
            }).Start();
        }
    }
}
