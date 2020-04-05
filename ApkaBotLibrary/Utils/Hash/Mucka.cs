using System;
using System.Collections.Generic;
using System.Text;

namespace ApkaBotLibrary.Utils.Hash
{
    class Mucka
    {
        public static string GenerateMucka()
        {
            return new Random().NextDouble().ToString("R");
        }
    }
}
