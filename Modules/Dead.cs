using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules
{
    public class Dead
    {
        public int ClientID;
        public string Symbol;
        public char Side;
        public int Quantity;
        public int OpenQuantity;
        public Dead(String mess)
        {
            string[] toks = mess.Split('|');
            Symbol = toks[1];
            ClientID = Int32.Parse(toks[2]);
            Side = toks[3][0];
            Quantity = Int32.Parse(toks[4]);
            OpenQuantity = Int32.Parse(toks[5]);
        }
    }
}
