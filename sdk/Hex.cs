using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Veridu
{
    class Hex
    {
        public static String Encode(byte[] bytes)
        {
            StringBuilder hex = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}
