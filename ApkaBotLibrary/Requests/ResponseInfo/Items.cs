using Newtonsoft.Json;
using System.Collections.Generic;

namespace ApkaBotLibrary.Requests.ResponseInfo
{
    public class Item
    {
        [JsonProperty(PropertyName = "name")]
        public string name { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "loc")]
        public string loc { get; set; }

        [JsonProperty(PropertyName = "icon")]
        public string icon { get; set; }

        [JsonProperty(PropertyName = "x")]
        public string x { get; set; }

        [JsonProperty(PropertyName = "y")]
        public string y { get; set; }

        [JsonProperty(PropertyName = "cl")]
        public string cl { get; set; }

        [JsonProperty(PropertyName = "pr")]
        public string pr { get; set; }

        [JsonProperty(PropertyName = "st")]
        public string st { get; set; }

        [JsonProperty(PropertyName = "stat")]
        public string stat { get; set; }
    }
}
