using ApkaBotLibrary.Requests;
using ApkaBotLibrary.Requests.ResponseInfo;
using ApkaBotLibrary.Utils.Settings;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;

namespace ApkaBotLibrary.Account
{
    public class Character
    {
        private Settings _settings = new Settings();

        public CancellationTokenSource cts = new CancellationTokenSource();

        public string FullName { get => $"[{World.ToUpper().Substring(1,1)}{World[2..^0]}]=> {Nick}({lvl}{Prof})"; }

        public bool Pause { get; set; } = true;
        //public Dictionary<string, MobileMap> Maps;

        public List<Item> Items;

        public List<string> ToSell { get; set; } = new List<string>();

        public uint Id;

        public string Nick;

        public uint lvl;

        public char Prof;

        [JsonProperty("db")]
        public string World;

        [JsonProperty("zycie")]
        public uint Health;

        [JsonProperty("max_zycie")]
        public uint MaxHealth;

        public int Stamina;

        //public char Plec;

        public ulong Exp;

        public string Clanname;

        //public long clan;

        //public long clan_rank;

        //public long uprawnienia;

        //public string icon;

        //public long last;

        public Settings GetSettings => _settings;
#nullable enable
        public string? Token { get; set; }
    }
}
