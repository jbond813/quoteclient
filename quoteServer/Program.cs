using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace quoteServer
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpListener tl = new TcpListener(5555);
            tl.Start();
            for(;;)
            {
                TcpClient cl = tl.AcceptTcpClient();
                readerThread(cl);
                string[] files = Directory.GetFiles(@"e:\spy");
                try {
                    foreach(string file in files)
                    {
                        Thread.Sleep(20);
                        byte[] bytes = File.ReadAllBytes(file);
                        cl.GetStream().Write(bytes, 0, bytes.Length);
                        cl.GetStream().Flush();
                        Console.WriteLine("wrote");
                    }
                }
                catch (Exception)
                {
                }
            }
        }
        static void readerThread(TcpClient cl)
        {
            new Thread((o) => {
                TcpClient cli = (TcpClient)o;
                byte[] buff = new byte[100];
                for (;;)
                {
                    try
                    {
                        cli.GetStream().Read(buff, 0, 100);
                        Console.WriteLine("read");
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("disconnected");
                        return;
                    }
                }
            }).Start(cl);
        }
    }
}
