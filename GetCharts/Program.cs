using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GetCharts
{
    class Program
    {
        public static string[] GetSymbols()
        {
            //List<string> symbols = new List<string>();
            //symbols.AddRange((from s in File.ReadAllLines(@"c:\users\scott\downloads\nasdaq.csv") select s.Split(',')[0]).Skip(1));
            //symbols.AddRange((from s in File.ReadAllLines(@"c:\users\scott\downloads\nyse.csv") select s.Split(',')[0]).Skip(1));
            //symbols.AddRange((from s in File.ReadAllLines(@"c:\users\scott\downloads\amex.csv") select s.Split(',')[0]).Skip(1));
            //symbols = (from s in symbols select s.Replace("\"", "")).ToList<string>();
            //return symbols.ToArray();
            string symbolstring = File.ReadAllText(@"c:\users\scott\downloads\SampleListStocks2.txt");
            char[] ss = { ' ' };
            string[] symbols = symbolstring.Split(ss, StringSplitOptions.RemoveEmptyEntries);
            return symbols;
        }
        static object lo = new object();
        static void t(string symbol)
        {
            string url = $"https://www.google.com/finance/historical?output=csv&q={symbol}";
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            try
            {
                string[] ans = new StreamReader(req.GetResponse().GetResponseStream()).ReadToEnd().Split('\n').Skip(1).Take(20).ToArray();
                double atr = (from a in ans select Double.Parse(a.Split(',')[2]) - Double.Parse(a.Split(',')[3])).Average();
                double lc = Double.Parse(ans[0].Split(',')[4]);
                int adv = (int)(from a in ans select Int32.Parse(a.Split(',')[5])).Average();
                atr = Math.Round(atr, 2);
                string si = $"{symbol} {atr} {adv} {lc}\r\n";
                Console.WriteLine(si);
                lock (lo)
                {
                    File.AppendAllText($@"e:\symbolinfo{DateTime.Now.ToString("yyyyMMdd")}.txt", si);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"can't download{symbol} {e.Message}");
            }

        }
        static void Main(string[] args)
        {
            Console.WriteLine(DateTime.Now.ToString("yyyyMMdd"));
            List<Task> tasks = new List<Task>();
            foreach (string symbol in GetSymbols())
            {
                tasks.Add(
                Task.Factory.StartNew(() =>
                {
                    t(symbol);
                }));
            }
            Task.WaitAll(tasks.ToArray());
        }
    }
}
