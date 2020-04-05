using Newtonsoft.Json;

namespace ApkaBotLibrary.Requests
{
    public class MobileMaps
    {
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        //[JsonProperty("done")]
        //public long Done { get; set; }

        [JsonProperty("next_map")]
        public int NextMap { get; set; }

        [JsonProperty("req_map")]
        public int ReqMap { get; set; }
    }
}
