using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApkaBotLibrary.Requests.ResponseInfo
{
    public class Chat
    {
        [JsonProperty(PropertyName = "k")]
        public int Type { get; set; }

        [JsonProperty(PropertyName = "nd")]
        public string Receiver { get; set; }

        [JsonProperty(PropertyName = "n")]
        public string Author { get; set; }

        [JsonProperty(PropertyName = "t")]
        public string Message { get; set; }
    }
}
