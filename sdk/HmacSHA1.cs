using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace Veridu
{
    class HmacSHA1
    {
        public static string Calculate(string data, string secret)
        {
            var encoding = new System.Text.UTF8Encoding();
            byte[] keyByte = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(data);
            using (var hmacsha = new HMACSHA1(keyByte))
            {
                byte[] hashmessage = hmacsha.ComputeHash(messageBytes);
                return HexEncode(hashmessage).ToLower();
            }
        }

        private static string HexEncode(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
