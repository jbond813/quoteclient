using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDPackets
{
    public class Snapshot
    {
        public string Symbol;
        public double Bid;
        public double Ask;
        public double Last;
        public double High;
        public double Low;
        public double Open;
        public double Close;
        public int Volume;
        public int BidSize;
        public int AskSize;
        private RespRefreshSymbolPacket rp;
        public Snapshot(RespRefreshSymbolPacket rp)
        {
            this.rp = rp;
            Volume = BitConverter.ToInt32(rp.rawPayload, 62);
            //int vol = BitConverter.ToInt32(rp.rawPayload, 66);
            Bid = rp.getPrice(rp.rawPayload, 70);
            Ask = rp.getPrice(rp.rawPayload, 78);
            Last = rp.getPrice(rp.rawPayload, 86);
            Low = rp.getPrice(rp.rawPayload, 118);
            High = rp.getPrice(rp.rawPayload, 110);
            Open = rp.getPrice(rp.rawPayload, 94);
            Close = rp.getPrice(rp.rawPayload, 102);
            BidSize = BitConverter.ToInt32(rp.rawPayload, 126);
            AskSize = BitConverter.ToInt32(rp.rawPayload, 130);

        }
        public override string ToString()
        {
            return $"REFL1 {DateTime.Now.ToString("HH:mm:ss.fff")} Bid {rp.Symbol} {Bid} ask {Ask} last {Last} hi {High} vol {Volume} lo {Low} close {Close} open {Open} bidsize {BidSize}  asksize {AskSize}";
        }
    }
}
