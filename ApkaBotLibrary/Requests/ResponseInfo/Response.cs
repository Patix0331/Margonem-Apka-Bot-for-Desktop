using ApkaBotLibrary.Account;
using ApkaBotLibrary.Requests.ResponseInfo;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ApkaBotLibrary.Requests.Response
{
    public class Response
    {
        //[JsonProperty("name2")]
        //private string CodeModel2 { set { CodeModel = value; } }
        //few json names to one object

        public string Ok { get; set; }

        public string Msg { get; set; }

        public Loot Loot { get; set; }

        [JsonProperty(PropertyName = "h")]
        public CharacterInfo Stats { get; set; }

        [JsonProperty(PropertyName = "t")]
        public string status { get; set; }

        [JsonProperty(PropertyName = "e")]
        public string Error { get; set; }

        [JsonProperty(PropertyName = "c")]
        public Dictionary<string, Chat> Messages { get; set; }

        public Dictionary<string, Character> Charlist { get; set; }

        public Dictionary<string, byte> Charspw { get; set; }

        public string mobile_token { get; set; }

        [JsonProperty(PropertyName = "mobile_maps")]
        public Dictionary<string, MobileMaps> MobileMaps { get; set; }

        [JsonProperty(PropertyName = "item")]
        public Dictionary<string, Item> Items { get; set; }
    }
}
