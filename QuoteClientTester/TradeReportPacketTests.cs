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
            Assert.AreEqual(p.MMID, " FT ");
            Assert.AreEqual(p.Size, 200);
        }

        [TestMethod()]
        public void ToStringTest()
        {
            Assert.AreEqual("Trade Report 08:54:06.971 200 SPY@249.07  FT ", getPacket().ToString());
        }
    }
}