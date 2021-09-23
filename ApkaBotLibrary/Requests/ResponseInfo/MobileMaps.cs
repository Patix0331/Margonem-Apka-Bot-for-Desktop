using Newtonsoft.Json;

namespace ApkaBotLibrary.Requests
{
    public class MobileMaps
    {
        public int Id { get; set; } = 2675;
        [JsonProperty("name")]
        public string Name { get; set; } = "Leśny Gąszcz [1-5]";

        [JsonProperty("done")]
        public int Done { get; set; } = 0;

        [JsonProperty("next_map")]
        public int NextMap { get; set; } = 2676;

        [JsonProperty("req_map")]
        public int ReqMap { get; set; } = 0;
    }
}
