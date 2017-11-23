using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDPackets
{
    public class StockImbalanceIndicator : Packet
    {
        public StockImbalanceIndicator(ushort len, ushort pt, byte[] pl)
        {
            rawPayload = pl;
            length = len;
            packetType = (PacketType)pt;
        }
        //unsigned int m_farPriceDollar;
        //unsigned int m_farPriceFraction;
        //unsigned int m_nearPriceDollar;
        //unsigned int m_nearPriceFraction;
        //unsigned int m_currentReferencePriceDollar;
        //unsigned int m_currentReferencePriceFraction;
        //unsigned int m_pairedShares;
        //unsigned int m_imbalanceShares;
        //unsigned int m_marketImbalanceShares;
        //unsigned int m_time;
        //unsigned int m_auctionTime;
        //char m_imbalanceDirection;
        //char m_crossType;
        //char m_priceVariationIndicator;
        //unsigned char m_bookID : 4;
        //unsigned char m_regulatoryImbalance_StockOpen : 1;


        //private Int64 referenceNumber;
        public override string Symbol { get { return ASCIIEncoding.ASCII.GetString(rawPayload, 2, 8).TrimEnd((Char)0); } }
        public Double FarPrice { get { return getPrice(rawPayload,10); } set { } }
        public Double NearPrice { get { return getPrice(rawPayload, 18); } set { } }
        public Double CurrentRefPrice { get { return getPrice(rawPayload, 26); } set { } }
        public int PariredShares { get { return BitConverter.ToInt32(rawPayload,34); } set { } }
        public int ImbalanceShares { get { return BitConverter.ToInt32(rawPayload, 38); } set { } }
        public int MarketImbalanceShares { get { return BitConverter.ToInt32(rawPayload, 42); } set { } }
        public DateTime Time { get { return getTime(rawPayload,46); } set { } }
        public DateTime AuctionTime { get { return getTime(rawPayload, 50); } set { } }
        public char ImbalanceDirection { get { return (char)rawPayload[54]; } set { } }
        public char CrossType { get { return (char)rawPayload[55]; } set { } }
        public char book_reg { get { return (char)rawPayload[55]; } set { } }

        public override string ToString()
        {

            StringBuilder sb = new StringBuilder();
            sb.Append(Symbol).Append("Imbal ").Append(FarPrice).Append(" ");
            sb.Append(NearPrice).Append(" ").Append(CurrentRefPrice).Append(" ");
            sb.Append(PariredShares).Append(" ").Append(ImbalanceShares).Append(" ");
            sb.Append(MarketImbalanceShares).Append(" ").Append(Time.ToString("HH:mm:ss.fff")).Append(" ");
            sb.Append(AuctionTime.ToString("HH:mm:ss.fff")).Append(" ").Append(ImbalanceDirection).Append(" ");
            sb.Append(CrossType).Append(" ").Append(book_reg).Append(" ");
            return sb.ToString();
        }
    }
}
