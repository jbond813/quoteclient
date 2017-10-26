using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuoteClientTester
{
    class Util
    {
        public static Stream PacketStreamCreator(string str)
        {
            byte[] arr = new byte[(str.Length / 2) + 2];
            short len = (short)(arr.Length);
            byte[] lena = BitConverter.GetBytes(len);
            arr[0] = lena[0];
            arr[1] = lena[1];
            for (int i = 0;i < arr.Length - 2;i++)
            {
                string s = str.Substring(i * 2, 2);
                byte b = Byte.Parse(s, System.Globalization.NumberStyles.HexNumber);
                arr[i + 2] = b;
            }
            MemoryStream ms = new MemoryStream(arr);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
    }
}
