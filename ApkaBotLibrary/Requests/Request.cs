using ApkaBotLibrary.Account;
using ApkaBotLibrary.Requests.ResponseInfo;
using ApkaBotLibrary.Utils.Hash;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace ApkaBotLibrary.Requests
{


    public class Request
    {
        #region Properties
        private HttpClient _apiClient;
        private HttpClientHandler _handler = new HttpClientHandler();
        private CookieContainer _cookies = new CookieContainer();
        private Uri _uri;
        private Random random { get; set; } = new Random();
        private async Task Wait(int min = 200, int max = 300)
        {
            await Task.Delay(random.Next(min, max));
        }

        /// <summary>
        /// Get cookies
        /// </summary>
        public IEnumerable<Cookie> GetCookies => _cookies.GetCookies(uri: new Uri("https://new.margonem.pl/ajax/login")).Cast<Cookie>();
        /// <summary>
        /// Set Cookies for specific address (character)
        /// </summary>
        public void SetCookies(Uri url, CookieCollection cookies) => _cookies.Add(url, cookies);
        /// <summary>
        /// Constructor
        /// </summary>
        public Request()
        {
            _handler.CookieContainer = _cookies;
            _apiClient = new HttpClient(_handler);
            _apiClient.DefaultRequestHeaders.Accept.Clear();
            _apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _apiClient.DefaultRequestHeaders.Add("User-Agent", "Dalvik/2.1.0 (Linux; U; Android 8.0.0; Samsung Galaxy S8 - 8.0 - API 26 - 1440x2960 Build/OPR6.170623.017)");
            _apiClient.DefaultRequestHeaders.Add("X-Unity-Version", "5.6.2p4");
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sign in into the game //POST
        /// </summary>
        /// <returns>boolean is logged properly</returns>
        public async Task<bool> Login(string login, string password)
        {
            this._uri = new Uri("https://new.margonem.pl/ajax/login");
            var values = new Dictionary<string, string>
            {
                { "l", login },
                { "p", password }
            };
            var content = new FormUrlEncodedContent(values);

            var response = await _apiClient.PostAsync(_uri, content).ContinueWith((response) =>
            {
                return response.Result.Content.ReadAsStringAsync();
            });
            Response.Response info = JsonConvert.DeserializeObject<Response.Response>(await response);

            return Convert.ToBoolean(Convert.ToByte(info.Ok));
        }

        /// <summary>
        /// Get and set list of characters into Apka class //GET
        /// </summary>
        /// <returns>Is succeeded</returns>
        public async Task<bool> Characters()
        {
            this._uri = new Uri("http://www.margonem.pl/ajax/getplayerdata.php?app_version=1.3.6");

            var response = await _apiClient.GetAsync(_uri).ContinueWith((response) =>
            {
                return response.Result.Content.ReadAsStringAsync();
            });
            Response.Response info = JsonConvert.DeserializeObject<Response.Response>(await response);

            if (info.Ok == "1")
            {
                Apka.GetInstance.SetCharacters(info.Charlist);
                foreach (var world in info.Charspw)
                {
                    Apka.GetInstance.AddNumberPerWorld(world.Key);
                }
            }

            return Convert.ToBoolean(Convert.ToByte(info.Ok));
        }

        /// <summary>
        /// Everything with initalization in game (4 reqeusts) //GET
        /// </summary>
        public async Task<bool> Init(Character player)
        {
            for (int level = 1; level <= 4; level++)
            {
                this._uri = new Uri(string
                    .Concat(new string[] {
                                    $"http://{player.World[1..^0]}",
                                    ".margonem.pl/engine?t=init&initlvl=",
                                    $"{level}",
                                    $"&mucka={Mucka.GenerateMucka()}",
                                    $"$aid={player.Id}",
                                    "&mobile=1",
                                    player.Token == null ? "" : $"&mobile_token={player.Token}"
                                }));

                //Apka.GetInstance.GetCookies().Add(url, new Cookie("mchar_id", characterId));
                _cookies.Add(this._uri, new Cookie("mchar_id", player.Id.ToString()));
                var response = await _apiClient.GetAsync(_uri).ContinueWith(response => response.Result.Content.ReadAsStringAsync().Result);

                if (response.Contains("Nie jesteś zalogowany"))
                {
                    throw new Exception("Nie jesteś zalogowany!");
                }

                Response.Response info = JsonConvert.DeserializeObject<Response.Response>(response);

                if (info.Ok == "0")
                {
                    return false;
                }
                #region LEVELS
                if (level == 1)
                {
                    //TODO get data from response
                    player.Token = MD5.Hash(info.mobile_token);
                    player.Stamina = info.Stats.Stamina;
                }
                else if (level == 2)
                {
                    if (Apka.GetInstance.Maps.Count == 0)
                        foreach (var map in info.MobileMaps)
                        {
                            map.Value.Id = map.Key;
                            Apka.GetInstance.Maps.Add(map.Value);
                        }
                }
                else if (level == 3)
                {
                    //fixme object references not to set of object
                    player.Items = info.Items.Values.ToList<Item>();
                }
                #endregion
            }
            await Task.Delay(50);

            return true;
        }

        /// <summary>
        /// Method managing of items
        /// </summary>
        public async Task<bool> Use(Character player, string action)
        {
            //TODO 

            return true;
        }

        /// <summary>
        /// Logic of healing character
        /// </summary>
        public async Task<bool> Heal(Character player)
        {
            int percantage = player.GetSettings.AutoHealHP;
            if (percantage == 0)
                return true;

            percantage -= percantage == 100 ? 1 : 0;

            string[] types = { "leczy", "perheal" };

            if (player.GetSettings.FullHeal)
                types[2] = "fullheal";

            var potions = player.Items.Where(x => x.cl == "16").Where(x => types.Any(type => x.stat.Contains(type))).ToList();

            while (100 * player.Health / player.MaxHealth <= percantage || potions.Count != 0)
            {

                bool used = await Get(player, $"moveitem&id={potions.First().Id}");

                if (used)
                {
                    player.Items.Remove(potions.First());
                    potions.Remove(potions.First());
                }
                await Wait();
            }

            return true;
        }

        /// <summary>
        /// Main HTTP GET REQUEST method
        /// </summary>
        /// <param name="action">URL get param "t"</param>
        public async Task<bool> Get(Character player, string action)
        {
            this._uri = new Uri(string
                    .Concat(new string[] {
                                    $"http://{player.World[1..^0]}",
                                    $".margonem.pl/engine?t={action}",
                                    $"&mucka={Mucka.GenerateMucka()}",
                                    $"&aid={player.Id}",
                                    "&mobile=1",
                                    $"&ev={new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()}",
                                    $"&mobile_token={player.Token}"
                                }));

            _cookies.Add(this._uri, new Cookie("mchar_id", player.Id.ToString()));

            var response = await _apiClient.GetAsync(_uri).ContinueWith(response => response.Result.Content.ReadAsStringAsync().Result);

            if (response.Contains("Nie jesteś zalogowany"))
            {
                throw new Exception("Nie jesteś zalogowany!");
            }

            Response.Response info;

            try
            {
                info = JsonConvert.DeserializeObject<Response.Response>(response);

            }
            catch (Exception)
            {
                return true;
            }

            //Get messages
            //TODO make this wiser
            try
            {
                if (info.Messages != null)
                    foreach (var message in info.Messages.Values)
                    {
                        Apka.GetInstance.Messages.Add(message);
                    }

                if (info.Error == "Sesja gry wygasła.")
                {
                    await Init(player);
                }

                if (info.Stats != null)
                {
                    if (info.Stats.Stamina != 0)
                        player.Stamina = info.Stats.Stamina;
                }
            }
            catch (Exception)
            {
                return true;
            }

            try
            {
                if (info.Loot != null)
                {
                    if (info.Loot.Items != null)
                    {
                        await Wait();
                        await Get(player, $"loot&not=&want=&must=&final=1");
                        foreach (var item in info.Loot.Items)
                        {
                            //player.ToSell.Add(item.Key);
                            await Wait();
                            await Get(player, $"shop&sell={item.Key}");
                        }
                    }
                }
            }
            catch (Exception)
            {

                return true;
            }


            return true;
        }

        /// <summary>
        /// Main method which managing player actions
        /// </summary>
        public async Task<bool> MainMethod(List<Character> players, CancellationToken cancellationToken)
        {
            foreach (var player in players)
            {
                await Wait();
                if (Apka.GetInstance.NumberPerWorld[player.World[1..^0]] || player.Pause)
                {
                    continue;
                }

                await Init(player);

                while (player.Stamina > 1)
                {
                    if (Apka.GetInstance.NumberPerWorld[player.World[1..^0]] || player.Pause)
                    {
                        continue;
                    }

                    await Get(player, $"fight&a=attack&town_id={player.GetSettings.Map.Id}");
                    await Wait();
                    await Get(player, "fight&a=f");
                    await Wait();
                    await Get(player, "fight&a=quit");
                    await Wait();

                    //Selling is in Get() method
                    await Heal(player);


                }

                if (players.Count != 1)
                    await Wait(10*100, 12*100);
            }

            return true;
        }
        #endregion
    }
}
