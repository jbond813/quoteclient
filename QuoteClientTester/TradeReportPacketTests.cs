using Microsoft.VisualStudio.TestTools.UnitTesting;
using MDPackets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuoteClientTester;
using System.IO;

namespace MDPackets.Tests
{
    [TestClass()]
    public class TradeReportPacketTests
    {
        public static TradeReportPacket getPacket()
        {
            string str = "12007BFFE80153505900000000002A03000000000000F9000000801D2C04C8000000204654205A508831F77F0000";
            Stream stream = Util.PacketStreamCreator(str);
            TradeReportPacket p = (TradeReportPacket)Packet.GetPacket(stream);
            return p;
        }
        [TestMethod()]
        public void TradeReportPacketTest()
        {
            TradeReportPacket p = getPacket();
            Assert.AreEqual(p.Price, 249.07);
            Assert.AreEqual(p.Time, DateTime.Now.Date.AddHours(8).AddMinutes(54).AddSeconds(06).AddMilliseconds(971));
            //Assert.AreEqual(p.MMID, " FT ");
            Assert.AreEqual(p.Size, 200);
            Assert.AreEqual(46,p.length,"length");
        }
        [TestMethod()]
        public void TradeReportPacketBuildTest()
        {
            TradeReportPacket p = BuildFakeData.BuildFakeTradeReportPacket("AAPL", DateTime.Now.Date.AddHours(8.5), 12345, 1.23, 123, DateTime.Now.Date.AddHours(8.5));
            Assert.AreEqual(p.Size, 123,"Size not equal");
            Assert.AreEqual(12345, p.ReferenceNumber,"Refno not equal");
            Assert.AreEqual(46, p.length, "length not equal");
            Assert.AreEqual(DateTime.Now.Date.AddHours(8.5), p.Time, "time not equal");
            Assert.AreEqual(1.23, p.Price, "Price not equal");
            Assert.AreEqual(DateTime.Now.Date.AddHours(8.5),p.Time2, "other time not equal");
            Assert.AreEqual("Trade Report 08:30:00.000 123 AAPL@1.23", p.ToString(), "TR To String");
        }

        [TestMethod()]
        public void TradeReportToStringTest()
        {
            Assert.AreEqual("Trade Report 08:54:06.971 200 SPY@249.07", getPacket().ToString(),"TradeReport To String");
        }
    }
}