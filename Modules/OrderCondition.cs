using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules
{
    public class OrderCondition
    {
        public enum TIF { DAY, OPG }
        public TIF Tif;
        public double Limit;
        public int Shares;
        public char Side;
    }
}
