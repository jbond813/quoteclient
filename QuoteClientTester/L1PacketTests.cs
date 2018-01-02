using Microsoft.VisualStudio.TestTools.UnitTesting;
using MDPackets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using QuoteClientTester;

namespace MDPackets.Tests
{
    [TestClass()]
    public class L1PacketTests
    {
        public static L1Packet getPacket()
        {
            string str = "1100DED8E8015350590000000000F900000000B4C404F900000000E1F505640000001027000050544D52";
            Stream stream = Util.PacketStreamCreator(str);
            L1Packet l1p = (L1Packet)Packet.GetPacket(stream);
            return l1p;
        }
        [TestMethod()]
        public void L1PacketTest()
        {
            //L1Packet l1p = getPacket();
            L1Packet l1p = BuildFakeData.BuildFakeL1Packet("AAPL", 100.02, 110.34, 500, 600, DateTime.Today.AddHours(8.5));
            Assert.AreEqual(l1p.Bid, 100.02);
            Assert.AreEqual(l1p.Ask, 110.34);
            Assert.AreEqual(l1p.BidSize, 500);
            Assert.AreEqual(l1p.AskSize, 600);
            DateTime dt = DateTime.Now.Date.AddHours(8).AddMinutes(30);
            Assert.AreEqual(l1p.Time, dt);            
        }

        [TestMethod()]
        public void ToStringTest()
        {
            Console.WriteLine(getPacket().ToString());
            string dt = DateTime.Now.Date.AddHours(8).AddMinutes(53).AddSeconds(57).AddMilliseconds(86).ToString("M/d/yyyy h:mm:ss tt");
            Console.WriteLine(dt);
            Assert.AreEqual("PacketType=M_LEVEL1 Symbol=SPY Bid=249.08 Ask=249.10 BidSize=100 AskSize=10000 Time=" + dt, getPacket().ToString());
        }
    }
}