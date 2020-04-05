using System;
using System.Collections.Generic;
using System.Text;

namespace ApkaBotLibrary.Utils.Hash
{
    internal class MD5
    {
        public static string Hash(string blank, string salt = "humantorch-")
        {
            byte[] array = System.Security.Cryptography.MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(salt + blank));
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
            {
                stringBuilder.Append(array[i].ToString("x2"));
            }
            return stringBuilder.ToString();
        }
    }
}
