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

        private Request _req = new Request();

        private Dictionary<string, Character> _characters = new Dictionary<string, Character>();

        private Dictionary<string, bool> _characterMaps { get; set; } = new Dictionary<string, bool>();

        public List<Chat> Messages = new List<Chat>();

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
        public Request Req => this._req;

        /// <summary>
        /// Add character to Dict of characters.
        /// </summary>
        public void SetCharacters(Dictionary<string, Character> characters) => _characters = characters;

        /// <summary>
        /// Get list of characters from this account.
        /// </summary>
        public Dictionary<string, Character> GetCharacters => _characters;

        /// <summary>
        /// Get character by ID.
        /// </summary>
        public Character GetCharacter(string id) => _characters[id];

        /// <summary>
        /// Set number of characters on the same world.
        /// </summary>
        public void AddNumberPerWorld(string numberPerWorld) => _characterMaps.Add(numberPerWorld, false);

        /// <summary>
        /// Get number of characters on the same world.
        /// </summary>
        public Dictionary<string, bool> NumberPerWorld => _characterMaps;
    }

}
