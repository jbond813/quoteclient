using MDPackets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace quoteclient
{
    class StatusListener
    {
        HttpListener list;
        bool done = false;
        void threadProc()
        {
            list = new HttpListener();
            list.Prefixes.Add("http://localhost:8881/go/");
            list.Start();
            while(!done)
            {
                HttpListenerContext ctx = list.GetContext();
                switch(ctx.Request.RawUrl)
                {
                    case "/go/stop":
                        SendStopPositions(ctx);
                        break;
                    default:
                        SendOpenPositions(ctx);
                        break;
                }
            }

        }
        private void SendStopPositions(HttpListenerContext ctx)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<html><table border=\"1\" style=\"width:100%\">");

            sb.Append($"<tr><th>Symbol</th><th>ADV</th><th>ATR</th><th>PrevClose</th><th>ExecutionPrice</th><th>ExecutionTime</th><th>Stop</th><th>OpenShares</th><th>StopID</th><th>StopMyId</th><th>LimitID</th><th>LimitMyId</th><th>LimitPrice</th></tr>");
            foreach (string key in dict.Keys)
            {
                QuoteHolder qh = dict[key];
                if (qh.StopOrderMyID != 0)
                {
                    sb.Append("<tr>");
                    sb.Append($"<td>{key}</td><td>{qh.ADV}</td><td>{qh.ATR}</td><td>{qh.PrevClose}</td><td>{qh.ExecutionPrice}</td><td>{qh.ExecutionTime.ToString("HH:mm:ss")}</td><td>{qh.Stop}</td><td>{qh.OpenShares}</td><td>{qh.StopOrderID}</td><td>{qh.StopOrderMyID}</td><td>{qh.LimitOrderID}</td><td>{qh.LimitOrderMyID}</td><td>{qh.LimitPrice}</td>");
                    sb.Append("</tr>");
                }
            }
            sb.Append("</table></html>");
            byte[] output = ASCIIEncoding.ASCII.GetBytes(sb.ToString());
            ctx.Response.OutputStream.Write(output, 0, output.Length);
            ctx.Response.Close();
        }
        private void SendOpenPositions(HttpListenerContext ctx)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<html><table border=\"1\" style=\"width:100%\">");

            sb.Append($"<tr><th>Symbol</th><th>ADV</th><th>ATR</th><th>PrevClose</th><th>ExecutionPrice</th><th>ExecutionTime</th><th>Stop</th><th>OpenShares</th><th>LimitID</th><th>MyId</th><th>LimitPrice</th></tr>");
            foreach (string key in dict.Keys)
            {
                QuoteHolder qh = dict[key];
                if (qh.OpenShares != 0)
                {
                    sb.Append("<tr>");
                    sb.Append($"<td>{key}</td><td>{qh.ADV}</td><td>{qh.ATR}</td><td>{qh.PrevClose}</td><td>{qh.ExecutionPrice}</td><td>{qh.ExecutionTime.ToString("HH:mm:ss")}</td><td>{qh.Stop}</td><td>{qh.OpenShares}</td><td>{qh.LimitOrderID}</td><td>{qh.LimitOrderMyID}</td><td>{qh.LimitPrice}</td>");
                    sb.Append("</tr>");
                }
            }
            sb.Append("</table></html>");
            byte[] output = ASCIIEncoding.ASCII.GetBytes(sb.ToString());
            ctx.Response.OutputStream.Write(output, 0, output.Length);
            ctx.Response.Close();
        }

        IDictionary<string, QuoteHolder> dict;
        public StatusListener(IDictionary<string,QuoteHolder> d)
        {
            dict = d;
            new Thread(threadProc).Start();
        }
    }
}
