using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules
{
    public class Order
    {
        public int MyID;
        public int TheirID;
        public Dead Dead = null;
        public double LimitPrice;
        public double AverageExecutionPrice;
        public OrderCondition Condition;
        public IEnumerable<Execution> Executions
        {
            get { return null; }
        }
    }
}
