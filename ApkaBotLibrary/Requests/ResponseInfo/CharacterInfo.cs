using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApkaBotLibrary.Requests.ResponseInfo
{
    public class CharacterInfo
    {
        private string Outfit;

        public string Img
        {
            get { return $"https://margonem.pl/obrazki/postacie{Outfit}"; }
            set { Outfit = value; }
        }


        public string id { get; set; }
        public int blockade { get; set; }
        //public int uprawnienia { get; set; }
        //public int ap { get; set; }
        public int bagi { get; set; }
        public int bint { get; set; }
        public int bstr { get; set; }
        public int credits { get; set; }
        //public int runes { get; set; }
        public ulong exp { get; set; }
        public ulong gold { get; set; }
        public ulong goldlim { get; set; }
        public int healpower { get; set; }
        public int honor { get; set; }
        public int lvl { get; set; }
        public int mails { get; set; }
        public int mails_all { get; set; }
        public string mails_last { get; set; }
        //public string mpath { get; set; }
        public string nick { get; set; }
        public int opt { get; set; }
        public Clan clan { get; set; }
        public char prof { get; set; }
        public int ttl_value { get; set; }
        public int ttl_end { get; set; }
        public int ttl_del { get; set; }
        public int pvp { get; set; }
        public int ttl { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public int dir { get; set; }
        public int stasis { get; set; }
        public int bag { get; set; }
        public int party { get; set; }
        public int trade { get; set; }
        public int wanted { get; set; }
        public int stamina { get; set; }
        public int stamina_ts { get; set; }
        public int stamina_renew_sec { get; set; }
        public int cur_skill_set { get; set; }
        public int cur_battle_set { get; set; }
        public int preview_acc { get; set; }
        public int attr { get; set; }

        public int? hp { get; set; }

        [JsonProperty(PropertyName = "warrior_stats")]
        public WarriorStats warrior_stats { get; set; } = new WarriorStats();
    }

    public class WarriorStats
    {
        [JsonProperty(PropertyName = "hp")]
        public int hp { get; set; }

        [JsonProperty(PropertyName = "maxhp")]
        public int maxhp { get; set; }
    }

    public class GetCharInfo
    {
        public string Id;

        public string Icon;

        public string Nick;

        public int lvl;

        public char Prof;

        [JsonProperty("db")]
        public string World;

        [JsonProperty("zycie")]
        public int Health;

        [JsonProperty("max_zycie")]
        public int MaxHealth;

        public int Stamina;

        public ulong Exp;

        public string Clanname;
    }

}
