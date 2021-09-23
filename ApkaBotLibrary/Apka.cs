using ApkaBotLibrary.Requests;
using System.Collections.Generic;
using ApkaBotLibrary.Account;
using Newtonsoft.Json;
using ApkaBotLibrary.Requests.ResponseInfo;

namespace ApkaBotLibrary
{
    public class Apka
    {
        private static Apka _instance;

        private Request _req;

        public Dictionary<string, Character> Characters = new Dictionary<string, Character>();

        private Dictionary<string, bool> _characterMaps { get; set; } = new Dictionary<string, bool>();

        //public List<Chat> Messages = new List<Chat>();

        public string Login { get; set; }

        /// <summary>
        /// List of mobile maps
        /// </summary>
        public List<MobileMaps> Maps = new List<MobileMaps>();

        /// <summary>
        /// Get instance of this class.
        /// </summary>
        public static Apka GetInstance => _instance ?? (_instance = new Apka());

        /// <summary>
        /// Get instance of Requests class.
        /// </summary>
        public Request Req => _req ?? (_req = new Request());

        public void ResetReq()
        {
            _req = new Request();
        }

        /// <summary>
        /// Get character by ID.
        /// </summary>
        public Character GetCharacter(string id) => Characters[id];

        /// <summary>
        /// Set number of characters on the same world.
        /// </summary>
        public void AddNumberPerWorld(string numberPerWorld) => _characterMaps.Add(numberPerWorld, false);

        /// <summary>
        /// Get number of characters on the same world.
        /// </summary>
        public Dictionary<string, bool> NumberPerWorld => _characterMaps;


        public string OnWhichCharacterAtm { get; set; } = "";
        public bool Exit { get; set; }
        public bool ParallelAttacking { get; set; }
    }

}
