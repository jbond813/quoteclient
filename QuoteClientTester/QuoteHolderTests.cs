using Microsoft.VisualStudio.TestTools.UnitTesting;
using MDPackets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MDPackets.Tests
{
    [TestClass()]
    public class QuoteHolderTests
    {
        [TestMethod()]
        public void PostExecTest()
        {
            QuoteHolder qh = new QuoteHolder("BG 1.21 183869 61.75");
            QuoteHolder.Logger = new StreamWriter(new MemoryStream());
            qh.PostExec("AAA", 100, 123.45);
            Assert.AreEqual(qh.ADV, 183869);
            Assert.AreEqual(qh.ATR, 1.21);
            Assert.AreEqual(qh.PrevClose, 61.75);
            Assert.AreEqual(qh.OpenShares, 100);
            Assert.AreEqual(qh.OrderSent, false);
            Assert.AreEqual(qh.Stop, 0);

        }

        [TestMethod()]
        public void QuoteHolderTest()
        {
            QuoteHolder qh = new QuoteHolder("BG 1.21 183869 61.75");
            QuoteHolder.Logger = new StreamWriter(new MemoryStream());
            qh.Resp.Add(RespRefreshSymbolPacketTests.getPacket());
            qh.L1 = L1PacketTests.getPacket();
            Assert.AreEqual(qh.ADV, 183869);
            Assert.AreEqual(qh.ATR, 1.21);
            Assert.AreEqual(qh.PrevClose, 61.75);
            Assert.AreEqual(qh.OpenShares, 0);
            Assert.AreEqual(qh.OrderSent, false);
            Assert.AreEqual(qh.Stop, 0);
        }

        [TestMethod()]
        public void ProcessTradeTest()
        {
            QuoteHolder qh = new QuoteHolder("BG 1.21 183869 61.75");
            QuoteHolder.Logger = new StreamWriter(new MemoryStream());
            qh.Resp.Add(RespRefreshSymbolPacketTests.getPacket());
            qh.L1 = L1PacketTests.getPacket();
            qh.ProcessTrade(TradeReportPacketTests.getPacket());


        }
    }
}