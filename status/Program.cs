using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace status
{
    class Program
    {
        enum StockRefreshSteps
        {
            SRS_DESCRIPTION = 1 << 0,
            SRS_ATTRIBUTES = 1 << 1,
            SRS_LEVEL1 = 1 << 2,
            SRS_LEVEL2 = 1 << 3,
            SRS_ADDITIONAL = 1 << 4,
            SRS_PRINTS = 1 << 5,
        }
        static void Main(string[] args)
        {
            int SRS_DESCRIPTION = 1 << 0;
            int SRS_ATTRIBUTES = 1 << 1;
            int SRS_LEVEL1 = 1 << 2;
            int SRS_LEVEL2 = 1 << 3;
            int SRS_ADDITIONAL = 1 << 4;
            int SRS_PRINTS = 1 << 5;
            Console.WriteLine($"SRS_DESCRIPTION {SRS_DESCRIPTION}");
            Console.WriteLine($"SRS_ATTRIBUTES {SRS_ATTRIBUTES}");
            Console.WriteLine($"SRS_LEVEL1 {SRS_LEVEL1}");
            Console.WriteLine($"SRS_LEVEL2 {SRS_LEVEL2}");
            Console.WriteLine($"SRS_ADDITIONAL {SRS_ADDITIONAL}");
            Console.WriteLine($"SRS_PRINTS {SRS_PRINTS}");

        }
    }
}
