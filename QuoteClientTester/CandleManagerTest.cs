using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TradingInterfaces;
using Modules;
using MDPackets;

namespace QuoteClientTester
{
    class CMTimeProvider : ITimeProvider
    {
        public DateTime dtr;
        public DateTime GetCurrentTime()
        {
            return dtr;
        }
    }
    [TestClass]
    public class CandleManagerTest
    {
        [TestMethod]
        public void TestCandleUpdate()
        {
            CMTimeProvider tp = new CMTimeProvider();
            CandleManager cm = new CandleManager(tp);
            TimeSpan ts = new TimeSpan(8, 30, 0);
            DateTime mid = DateTime.Now.Date;
            TradeReportPacket trp = BuildFakeData.BuildFakeTradeReportPacket("AAPL", mid + ts, 111, 110.10, 100, mid + ts);
            tp.dtr = mid + ts;
            cm.Update(trp);
            Assert.AreEqual(110.10, cm.Get(0).Open);
            cm.Update(trp);
            Assert.AreEqual(110.10, cm.Get(0).Open);
            Assert.AreEqual(200, cm.Get(0).Volume);
            Assert.AreEqual(null, cm.Get(1));
            trp = BuildFakeData.BuildFakeTradeReportPacket("AAPL", mid + ts, 111, 120.10, 100, (mid + ts).AddMinutes(5));
            cm.Update(trp);
            tp.dtr = (mid + ts).AddMinutes(5);
            Assert.AreNotEqual(null, cm.Get(1));
            Assert.AreEqual(120.10, cm.Get(0).Close);
            Assert.AreEqual(110.10, cm.Get(1).Close);
            Assert.AreEqual(110.10, cm.Get(4).Close);
            Assert.AreEqual(null, cm.Get(8));
        }
    }
}
