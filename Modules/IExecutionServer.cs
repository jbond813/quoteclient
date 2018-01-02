using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules
{
    public interface IExecutionServer
    {
        //int SendOrder(char side, string symbol, double LimitPrice, double Bid, double Ask, string TIF, int qty);
        Order SendOrder(OrderCondition cond, Stock s);
        void CancelOrder(Order o);
    }
}
