using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MDPackets;

namespace Modules
{
    public class Order
    {
        public string Symbol;
        public int MyID;
        public int TheirID;
        public OrderDead Dead = null;
        public double LimitPrice;
        public double AverageExecutionPrice;
        public OrderCondition Condition;
        public bool CancelSent = false;
        public IEnumerable<OrderExecuted> Executions
        {
            get { return null; }
        }
    }
}
