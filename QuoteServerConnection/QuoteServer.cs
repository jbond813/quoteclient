﻿using MDPackets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MarketData
{
    public class QuoteServer
    {
        class config
        {
            public static IPEndPoint quoteServerEP = new IPEndPoint(IPAddress.Parse("10.101.3.206"), 19100);
        }
        BlockingCollection<Packet> Queue;

    Stream str;
        void Write(byte[] b)
        {
            str.Write(b, 0, b.Length);
            str.Flush();
        }
        byte[] connectRequest = { 0x74, 0x00, 0xe9, 0x03, 0x47, 0x41, 0x4a, 0x42, 0x00, 0x74, 0x72, 0x61, 0x64, 0x65, 0x35, 0x35, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x31, 0x36, 0x39, 0x2e, 0x32, 0x35, 0x34, 0x2e, 0x31, 0x35, 0x36, 0x2e, 0x38, 0x38, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x31, 0x2e, 0x30, 0x2e, 0x33, 0x2e, 0x36, 0x35, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x5f, 0xeb, 0xd1, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        byte[] refreshRequest = { 0x14, 0x00, 0x0d, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x00, 0x00, 0x00, 0x28, 0x6b, 0xee };
        byte[] key = { 0x08, 0x00, 0x0eb, 0x03, 0x03, 0x00, 0x00, 0x00 };

        public QuoteServer(BlockingCollection<Packet> queue)
        {
            Queue = queue;
            TcpClient cl = new TcpClient();
            cl.Connect(config.quoteServerEP);
            str = cl.GetStream();
            Write(key);
            Write(connectRequest);
            Packet.GetPacket(str);
            new Thread(t => {
                for (;;)
                {
                    Packet p = Packet.GetPacket(str);
                    Console.WriteLine(p.packetType);
                    Queue.Add(p);
                }
            }).Start();
        }
        public void Subcribe(string symbol)
        {
            byte[] req = new byte[refreshRequest.Length];
            Array.Copy(refreshRequest, req, req.Length);
            Array.Copy(ASCIIEncoding.ASCII.GetBytes(symbol), 0, req, 4, symbol.Length);
            Write(req);
        }
    }
}
