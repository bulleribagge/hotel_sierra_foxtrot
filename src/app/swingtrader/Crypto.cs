using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace swingtrader
{
    public static class Crypto
    {
        public static string CalculateHmacSha256(string input, string key)
        {
            byte[] keyBytes = Encoding.ASCII.GetBytes(key);
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);

            using (HMACSHA256 mac = new HMACSHA256(keyBytes))
            {
                var resBytes = mac.ComputeHash(inputBytes);
                var resHex = BitConverter.ToString(resBytes).Replace("-", "").ToLower();
                return resHex;
            }
        }
    }
}
