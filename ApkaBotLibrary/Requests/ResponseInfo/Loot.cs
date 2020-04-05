using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApkaBotLibrary.Requests.ResponseInfo
{
    public class Loot
    {
        [JsonProperty("states")]
        public Dictionary<string, int> Items { get; set; }
    }
}
