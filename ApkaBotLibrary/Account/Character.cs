using ApkaBotLibrary.Requests;
using ApkaBotLibrary.Requests.ResponseInfo;
using ApkaBotLibrary.Utils.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ApkaBotLibrary.Account
{
    public class Character
    {
        private SettingsModel _settings = new SettingsModel();

        public CancellationTokenSource cts = new CancellationTokenSource();

        public string FullName { get => $"[{World}] {h.nick}({h.lvl}{h.prof})"; }

        public bool Pause { get; set; } = false;

        public bool Status { get; set; } = false;

        public SettingsModel GetSettings => _settings;

        public void SetSettings(SettingsModel value) => _settings = value;

        public Dictionary<string, Item> ToSell { get; set; } = new Dictionary<string, Item>();

        public Dictionary<string, Item> Items { get; set; } = new Dictionary<string, Item>();

        public List<MobileMaps> Maps { get; set; } = new List<MobileMaps>();

        [JsonProperty("db")]
        public string World;

        public CharacterInfo h { get; set; } = new CharacterInfo();

        #region INFO STATS
        public bool SuccessUnblocking { get; set; } = true;

        public int BlessLeft { get; set; }

        public int ArrowsLeft { get; set; }
        public int HealPowerLeft { get; set; }

        public Dictionary<DateTime, Item> SoldItems { get; set; } = new Dictionary<DateTime, Item>();

        public string StatusInfo { get; set; } = "Stopped";

        public int FreeSpaceLeft { get; set; }
        public bool FirstTime { get; set; } = true;
        public ulong StartGold { get; set; }
        public ulong StartExp { get; set; }
        public int StaminaCounter { get; set; }
        public int DeathCounter { get; set; }
        public bool IsDead { get; set; }
        #endregion

        public string BrowserToken { get; set; }
#nullable enable
        public string? Token { get; set; }
    }
}
