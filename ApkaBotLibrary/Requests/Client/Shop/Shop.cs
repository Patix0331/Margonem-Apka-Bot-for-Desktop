using System;
using System.Collections.Generic;
using System.Text;

namespace ApkaBotLibrary.Requests.Client.Shop
{
    public class Shop
    {
        public bool SellAtNpc { get; set; }
        public int NpcId { get; set; }
        public string ShopMessage { get; set; }
    }
}
