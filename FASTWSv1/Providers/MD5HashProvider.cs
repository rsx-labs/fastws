using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

using System.Security.Cryptography;

namespace FASTWSv1.Providers
{
    public static class MD5HashProvider
    {
        public static string CreateMD5Hash(string data)
        {
            using ( MD5 hash = MD5.Create())
            {
                return GetMD5Hash(hash, data);
            }
        }

        private static string GetMD5Hash(MD5 hash, string data)
        {

            byte[] input = hash.ComputeHash(Encoding.UTF8.GetBytes(data));

            StringBuilder sBuilder = new StringBuilder();

            for(int i= 0; i < input.Length ; i++)
            {
                sBuilder.Append(input[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }
    }
}