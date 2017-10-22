using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
class Program
{
    static void Main(string[] args)
    {
        Boolean done = false;
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IPAddress send_to_address = IPAddress.Parse("127.0.0.1");
        IPEndPoint sending_end_point = new IPEndPoint(send_to_address, 5555);
        socket.SendTo(ASCIIEncoding.ASCII.GetBytes("INIT"), sending_end_point);
        byte[] buff = new byte[1000];
        EndPoint rep = new IPEndPoint(IPAddress.Any, 0);
        int n = socket.ReceiveFrom(buff, ref rep);
        Console.WriteLine(ASCIIEncoding.ASCII.GetString(buff, 0, n));
        //socket.SendTo(ASCIIEncoding.ASCII.GetBytes("TRADE"), sending_end_point);
        string sym = "FB";
        double Close = 123.45;
        int volume = 23000000;
        double atr = 1.02;
        double targetPrice = 175.99;
        double bid = 175.67;
        double ask = 175.96;
        //string m = $"B|{sym}|{Close}|{volume}|{atr.ToString("0.00")}|{targetPrice.ToString("0.00")}|{bid.ToString("0.00")}|{ask.ToString("0.00")}|DAY";
        //socket.SendTo(Encoding.ASCII.GetBytes(m), sending_end_point);

        while (!done)
        {
            //socket.SendTo(Encoding.ASCII.GetBytes("BUY 100 AAPL 150.00"), sending_end_point);
            n = socket.ReceiveFrom(buff, ref rep);
            Console.WriteLine(ASCIIEncoding.ASCII.GetString(buff, 0, n));
        }
    }
}