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
using System.Text;
using System.Threading.Tasks;

namespace ApkaBotLibrary.Requests
{
    public partial class Request
    {
        #region Properties
        private HttpClient _apiClient;
        private HttpClientHandler _handler = new HttpClientHandler();
        private CookieContainer _cookies = new CookieContainer();
        private Uri _uri;
        private Random random { get; set; } = new Random();

        private string UserID => _cookies.GetCookies(new Uri("http://www.margonem.pl/")).FirstOrDefault(x => x.Name == "user_id").Value;

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
        /// Sign in into the game
        /// </summary>
        /// <returns>Is logged</returns>
        public async Task<int> Login(string login, string password, WebProxy proxy = null)
        {
            _uri = new Uri("https://new.margonem.pl/ajax/login");

            if (proxy != null)
            {
                _handler.Proxy = proxy;
            }

            var values = new Dictionary<string, string>
            {
                { "l", login },
                { "p", password }
            };
            var content = new FormUrlEncodedContent(values);

            HttpResponseMessage response;
            try
            {
                response = await _apiClient.PostAsync(_uri, content);
            }
            catch (Exception)
            {
                return -1;
            }

            var result = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return -1;
            }
            else if (result != "{\"ok\":1}")
            {
                return 0;
            }

            return 1;
        }

        /// <summary>
        /// Set <seealso cref="Apka.Characters"/> and <seealso cref="Apka.NumberPerWorld"/>
        /// </summary>
        public async Task<bool> Characters()
        {
            _uri = new Uri("http://www.margonem.pl/ajax/getplayerdata.php?app_version=1.3.6");

            string response = await _apiClient.GetStringAsync(_uri);
            Response.Response info = JsonConvert.DeserializeObject<Response.Response>(response);

            if (info.Ok == "1")
            {
                foreach (var item in info.Charlist)
                {
                    if (item.Value.World == "#experimental")
                    {
                        continue;
                    }

                    Apka.GetInstance.Characters.Add(item.Key, new Character());

                    var stats = Apka.GetInstance.Characters[item.Key];
                    stats.World = item.Value.World[1..^0];

                    stats.h.exp = item.Value.Exp;
                    stats.StartExp = stats.h.exp;

                    stats.h.warrior_stats.hp = item.Value.Health;
                    stats.h.id = item.Value.Id;
                    stats.h.lvl = item.Value.lvl;
                    stats.h.warrior_stats.maxhp = item.Value.MaxHealth;
                    stats.h.nick = item.Value.Nick;
                    stats.h.prof = item.Value.Prof;
                    stats.h.stamina = item.Value.Stamina;
                    stats.h.Img = item.Value.Icon;
                }

                foreach (var world in info.Charspw)
                {
                    if (world.Key == "Experimental")
                    {
                        continue;
                    }

                    Apka.GetInstance.AddNumberPerWorld(world.Key);
                }
            }

            return Convert.ToBoolean(Convert.ToByte(info.Ok));
        }

        /// <summary>
        /// Set <see cref="Character.Maps"/>, <see cref="Character.Token"/>, <see cref="Character.h"/>, <see cref="Character.Items"/>, <see cref="Character.FreeSpaceLeft"/>
        /// </summary>
        public async Task<bool> Init(Character player)
        {
            _cookies.Add(new Uri($"http://{player.World}.margonem.pl/"), new Cookie("mchar_id", player.h.id));

            for (int level = 1; level <= 4; level++)
            {
                var uri = new Uri(string
                    .Concat(new string[] {
                                    $"http://{player.World}",
                                    ".margonem.pl/engine?t=init&initlvl=",
                                    $"{level}",
                                    $"&mucka={Mucka.GenerateMucka()}",
                                    $"&aid={player.h.id}",
                                    "&mobile=1",
                                    player.Token == null ? "" : $"&mobile_token={player.Token}"
                                }));

                string response = await _apiClient.GetStringAsync(uri);

                if (response.Contains("Nie jesteś zalogowany"))
                {
                    throw new Exception(response);
                }

                if (response.Contains("Przerwa techniczna - aktualizacja silnika."))
                {
                    player.StatusInfo = "Przerwa techniczna";
                    await Wait(60 * 1000, 60 * 2000);
                    await Init(player);
                    return false;
                }

                if (response.Contains("wait_for"))
                {
                    await Task.Delay(5000);
                    await Init(player);
                    return false;
                }

                Response.Response info = JsonConvert.DeserializeObject<Response.Response>(response);

                if (info.Ok == "0")
                {
                    throw new Exception("Error info.Ok == 0");
                }
                #region LEVELS
                if (level == 1)
                {
                    player.Token = MD5.Hash(info.mobile_token);
                    player.h = info.Stats;

                    if (player.FirstTime)
                    {
                        player.FirstTime = false;
                        player.StartExp = player.h.exp;
                        player.StartGold = player.h.gold;
                    }
                }
                else if (level == 2)
                {
                    if (player.Maps.Count == 0)
                    {
                        foreach (var map in info.MobileMaps)
                        {
                            map.Value.Id = Convert.ToInt32(map.Key);
                            player.Maps.Add(map.Value);
                        }
                    }
                }
                else if (level == 3)
                {
                    int freeSpaceLeft = 0;
                    foreach (var item in info.Items)
                    {
                        //equipment on self
                        if (item.Value.st != 0)
                        {
                            //Items bag, cl==24 means bag, btype=18 means bag for keys.
                            if (item.Value.cl == "24" && !item.Value.stat.Contains("btype=18"))
                            {
                                foreach (var stat in item.Value.stat.Split(';'))
                                {
                                    if (stat.Split('=')[0] == "bag")
                                    {
                                        freeSpaceLeft += int.Parse(stat.Split('=')[1]);
                                        break;
                                    }
                                }
                            }
                            //cl == 25 -> bless
                            else if (item.Value.cl == "25")
                            {
                                foreach (var stat in item.Value.stat.Split(';'))
                                {
                                    if (stat.Split('=')[0] == "ttl")
                                    {
                                        player.BlessLeft = int.Parse(stat.Split('=')[1]);
                                        break;
                                    }
                                }
                            }

                            continue;
                        }

                        //Items in keys bag
                        else if (item.Value.y >= 36)
                        {
                            continue;
                        }

                        //Rest of items. We want to count them to have the remaining free space in your bags.
                        freeSpaceLeft--;
                    }
                    player.FreeSpaceLeft = freeSpaceLeft;

                    player.Items = info.Items;
                }
                #endregion
            }
            await Task.Delay(50);

            return true;
        }

        /// <summary>
        /// The method that manages the use of <see cref="Character.Items"/>
        /// </summary>
        public async Task Use(Character player, string action)
        {
            if (action == "arrow")
            {
                var myArrows = player.Items
                    .Where(x => x.Value.st != 0 && x.Value.cl == "21")
                    .FirstOrDefault();

                if (myArrows.Value == null)
                {
                    player.ArrowsLeft = 0;
                    return;
                }

                int ammoLeft = 0;
                foreach (var item in myArrows.Value.stat.Split(';'))
                {
                    if (item.Contains("ammo"))
                    {
                        ammoLeft = int.Parse(item.Split('=')[1]);
                        player.ArrowsLeft = ammoLeft;
                        break;
                    }
                }

                if (ammoLeft != 0)
                {
                    var arrow = player.Items
                            .Where(x => x.Value.stat.Contains("ammo") && x.Value.st == 0 && x.Value.cl == "21")
                            .Where(x => x.Value.name == myArrows.Value.name);


                    if (arrow.FirstOrDefault().Key == null)
                    {
                        player.StatusInfo = "Brak strzał";
                        return;
                    }

                    int fullAmmoLeft = 0;
                    foreach (var item in arrow)
                    {
                        foreach (var stat in item.Value.stat.Split(';'))
                        {
                            if (stat.Contains("ammo"))
                            {
                                fullAmmoLeft += int.Parse(stat.Split('=')[1]);
                                break;
                            }
                        }
                    }

                    player.ArrowsLeft = ammoLeft + fullAmmoLeft;

                    if (ammoLeft <= 50)
                    {
                        await Get(player, $"moveitem&id={arrow.First().Key}&st=1");
                    }
                }
                else
                {
                    player.ArrowsLeft = 0;
                    player.StatusInfo = "Brak strzał";
                    player.Pause = true;
                }
            }
            else if (action == "stamina" && player.GetSettings.UseStamina)
            {
                var potionOfStamina = player.Items.Where(x => x.Value.stat.Contains("stamina")).FirstOrDefault();
                if (potionOfStamina.Key == null)
                {
                    return;
                }

                await Get(player, $"moveitem&id={potionOfStamina.Key}&st=1");
            }

            return;
        }

        public async Task<string> getRequest(Character player, string action, bool mucka = true, bool mobile = true)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(string.Concat(new string[] {
                $"http://{player.World}",
                $".margonem.pl/engine?t={action}",
                mucka ? $"&mucka={Mucka.GenerateMucka()}" : "",
                mobile ? $"&aid={player.h.id}" : $"&aid={UserID}",
                mobile ? "&mobile=1" : "",
                $"&ev={new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()}",
                mobile ? $"&mobile_token={player.Token}" : $"&browser_token={player.BrowserToken}"
                }))
            };

            if (!mobile)
            {
                request.Headers.Referrer = new Uri($"http://{player.World}.margonem.pl/");
            }

            HttpResponseMessage response;
            Request:
            try
            {
                response = await _apiClient.SendAsync(request);
            }
            catch (Exception)
            {
                throw;
            }
            
            while (!response.IsSuccessStatusCode)
            {
                player.StatusInfo = "Błąd. Powtarzam akcję.";
                await Wait(500, 700);
                goto Request;
            }

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Logic of healing character
        /// </summary>
        public async Task<bool> Heal(Character player)
        {
            int percantage = player.GetSettings.AutoHealHP;
            if (percantage == 0)
            {
                return true;
            }

            percantage -= percantage == 100 ? 1 : 0;

            string[] types = { "leczy" };

            //if (player.GetSettings.FullHeal)
            //    types[2] = "fullheal";


            var potions = player.Items
                .Where(x => x.Value.cl == "16")
                .Where(x => types.Any(type => x.Value.stat.Contains(type)))
                .ToDictionary(x => x.Key, x => x.Value);

            while (100 * player.h.warrior_stats.hp / player.h.warrior_stats.maxhp <= percantage)
            {
                potions = player.Items
                    .Where(x => x.Value.cl == "16")
                    .Where(x => types.Any(type => x.Value.stat.Contains(type)))
                    .ToDictionary(x => x.Key, x => x.Value);

                if (potions.Count() == 0)
                {
                    break;
                }

                player.StatusInfo = "Leczy";

                await Get(player, $"moveitem&id={potions.First().Key}&st=1", true);

                await Wait();
            }

            int healPowerLeft = 0;

            foreach (var potion in potions.Values)
            {
                int amount = 0;
                foreach (var stats in potion.stat.Split(';'))
                {
                    if (stats.Split('=')[0] == "amount")
                    {
                        amount = int.Parse(stats.Split('=')[1]);
                    }
                    else if (stats.Split('=')[0] == "leczy")
                    {
                        healPowerLeft += amount * int.Parse(stats.Split('=')[1]);
                    }
                }
            }
            player.HealPowerLeft = healPowerLeft;

            if (player.h.warrior_stats.hp <= 1 && player.HealPowerLeft == 0)
            {
                player.StatusInfo = "BRAK POTEK!!!";
                player.Pause = true;
            }

            return true;
        }

        /// <summary>
        /// Main <see cref="Request._apiClient"/> method
        /// </summary>
        /// <param name="action">URL get param "t"</param>
        public async Task Get(Character player, string action, bool getInfo = false, bool mucka = false, bool mobile = true)
        {
            string response = await getRequest(player, action, mucka, mobile);

            await ResponseManager(player, response, getInfo, action);

            return;
        }

        /// <summary>
        /// Response manager decides what to do based on the <see cref="Response.Response"/>
        /// </summary>
        private async Task ResponseManager(Character player, string response, bool getInfo, string action)
        {
            Response.Response info;

            if (response.Contains("Nie jesteś zalogowany"))
            {
                throw new Exception($"{response}\n{action}");
            }

            if (response.Contains("Musisz najpierw zaliczyć poprzednią lokację!"))
            {
                await UnblockMap(player);

                response = await _apiClient.GetStringAsync(_uri);
            }

            try
            {
                info = JsonConvert.DeserializeObject<Response.Response>(response);
            }
            catch (Exception)
            {
                return;
            }

            //Get messages
            //TODO make this wiser
            try
            {
                if (info.Error == "Sesja gry wygasła." || response.Contains("reload"))
                {
                    await Init(player);
                }

                if (info.Stats != null)
                {
                    if (info.Stats.stamina != 0)
                    {
                        player.h.stamina = info.Stats.stamina;
                    }
                }
                if (getInfo)
                {
                    if (response.Contains("warrior_stats"))
                    {
                        player.h = info.Stats;
                    }
                    else
                    {
                        //TODO: move to individual method and clean up
                        if (response.Contains("dead"))
                        {
                            player.StatusInfo = $"Respie {info.dead}s";

                            if (!player.IsDead)
                            {
                                player.IsDead = true;
                                player.DeathCounter++;
                                player.h.warrior_stats.hp = 1;

                                if (action.Contains("boss_fight=1"))
                                {
                                    player.SuccessUnblocking = false;
                                }
                            }

                            await Wait(500, 1000);

                            await Get(player, "_", true);

                            return;
                        }
                        else if (player.IsDead)
                        {
                            player.IsDead = false;
                        }

                        if (response.Contains("hp"))
                        {
                            player.h.warrior_stats.hp = info.Stats.hp + 1 ?? player.h.warrior_stats.hp;
                            if (info.Stats.hp == 0 && !player.IsDead)
                            {
                                if (action.Contains("boss_fight=1"))
                                {
                                    player.SuccessUnblocking = false;
                                }

                                player.DeathCounter++;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return;
            }

            if (info.Loot != null)
            {
                if (info.Loot.Items != null)
                {
                    await LootManager(player, info.Loot, info.Items);
                    return;
                }
            }

            if (info.Items != null)
            {
                foreach (var item in info.Items)
                {
                    if (item.Value.Del == 1)
                    {
                        player.Items.Remove(item.Key);
                        player.FreeSpaceLeft++;
                    }
                    else
                    {
                        player.Items[item.Key] = item.Value;
                    }
                }
            }
        }

        /// <summary>
        /// <see cref="Loot"/> and selling manager
        /// </summary>
        /// <param name="player">Character object</param>
        /// <param name="Loot">loot json</param>
        /// <param name="Items">items json</param>
        private async Task LootManager(Character player, Loot Loot, Dictionary<string, Item> Items)
        {
            if (Loot.Items == null)
            {
                return;
            }

            if (Loot.FreeSpaceLeft == 0)
            {
                player.StatusInfo = "Brak miejsca na itemy";

                return;
            }

            player.FreeSpaceLeft = Loot.FreeSpaceLeft - Loot.Items.Count();

            player.StatusInfo = "Zbiera itemy";

            foreach (var item in Loot.Items.Keys)
            {
                player.ToSell.Add(item, Items[item]);

                if (!player.GetSettings.SellingPotions)
                {
                    if (Items[item].stat.Contains("leczy"))
                    {
                        var healPower = Items[item].stat.Split(';')
                            .Where(x => x.Split(';')[0] == "leczy")
                            .Select(x => int.Parse(x.Split('=')[1]))
                            .FirstOrDefault();

                        if (healPower == 0)
                        {
                            continue;
                        }
                        else if (healPower < player.GetSettings.MinValueOfHeal)
                        {
                            continue;
                        }
                        else
                        {
                            player.ToSell.Remove(item);
                        }
                    }
                    else if (Items[item].stat.Contains("fullheal"))
                    {
                        player.ToSell.Remove(item);
                    }
                }
                else if (Items[item].stat.Contains("stamina"))
                {
                    player.ToSell.Remove(item);
                }
                else if (Items[item].stat.Contains("gold"))
                {
                    player.ToSell.Remove(item);

                    await Get(player, $"moveitem&id={item}&st=1");
                    await Wait();
                }
                else if (Items[item].stat.Contains("quest"))
                {
                    player.ToSell.Remove(item);
                }
            }

            await Wait();
            await Get(player, $"loot&not=&want=&must=&final=1");
            await Init(player);
        }

        /// <summary>
        /// Selling items from <seealso cref="Character.ToSell"/>
        /// </summary>
        private async Task Sell(Character player)
        {
            if (player.GetSettings.ClientShop.SellAtNpc)
            {
                await MainClientMethod(player);
            }

            if (player.ToSell.Count <= 0)
            {
                return;
            }

            StringBuilder toSell = new StringBuilder();
            toSell.AppendJoin(',', player.ToSell.Keys);

            player.StatusInfo = $"Sprzedaję {player.ToSell.Count} itemków";
            foreach (var item in player.ToSell)
            {
                player.SoldItems.Add(DateTime.Now, item.Value);
                player.ToSell.Remove(item.Key);
            }

            await Get(player, $"shop&sell={toSell}");
            await Wait();
        }

        /// <summary>
        /// Unblock not awailable <seealso cref="Character.Maps"/>
        /// </summary>
        private async Task UnblockMap(Character player)
        {
            var highestUnblockedMap = player.Maps.Where(x => x.Done == 1).OrderBy(x => x.Id).LastOrDefault();
            if (highestUnblockedMap == null)
            {
                highestUnblockedMap = player.Maps.OrderBy(x => x.Id).First();
            }

            await Wait(1000, 1100);
            while (true)
            {
                //var nextMap = player.Maps.Where(x => x.Id == highestUnblockedMap.Id).First();
                player.StatusInfo = $"Odblokowuję {highestUnblockedMap.NextMap}";

                if (player.Pause)
                {
                    player.StatusInfo = "Zatrzymany";
                    return;
                }

                await Get(player, $"fight&a=attack&town_id={highestUnblockedMap.Id}&boss_fight=1", mucka: true);
                await Wait(1000, 1100);
                await Get(player, "fight&a=f");
                await Wait(1000, 1100);
                await Get(player, "fight&a=quit", true, mucka: true);
                await Wait(1000, 1100);

                await Heal(player);

                if (!player.SuccessUnblocking)
                {
                    player.SuccessUnblocking = true;
                    continue;
                }

                highestUnblockedMap.Done = 1;

                if (highestUnblockedMap.Id == player.GetSettings.Map.Id)
                {
                    return;
                }

                highestUnblockedMap = player.Maps.FirstOrDefault(x => x.Id == highestUnblockedMap.NextMap);
            }
        }

        /// <summary>
        /// Main method which managing player actions
        /// </summary>
        public async Task MainMethod(List<Character> players)
        {
            foreach (var player in players)
            {
                await Wait();
                if (player.Pause)
                {
                    player.StatusInfo = "Zatrzymany";
                    continue;
                }

                Apka.GetInstance.OnWhichCharacterAtm = player.FullName;
                //await GetReward(player);
                await Init(player);

                while (player.h.stamina > 1)
                {
                    player.Status = true;

                    if (player.Pause)
                    {
                        await Sell(player);

                        await SendGold(player);

                        player.StatusInfo = "Zatrzymany";
                        return;
                    }

                    if (player.GetSettings.UseBless && player.BlessLeft == 0)
                    {
                        var bless = player.Items.FirstOrDefault(x => x.Value.name == player.GetSettings.BlessName);
                        await Get(player, $"moveitem&id={bless.Key}&st=1");
                    }

                    player.StatusInfo = "Atakuje";
                    await Get(player, $"fight&a=attack&town_id={player.GetSettings.Map.Id}", mucka: true);
                    await Wait(1000, 1100);
                    await Get(player, "fight&a=f");
                    await Wait(1000, 1100);
                    await Get(player, "fight&a=quit", true, mucka: true);
                    await Wait(1000, 1100);

                    if (player.FreeSpaceLeft <= 5)
                    {
                        await Sell(player);
                    }

                    await Heal(player);

                    if (player.h.stamina <= 5)
                    {
                        await Use(player, "stamina");
                    }

                    if (player.h.prof == 'h' || player.h.prof == 't')
                    {
                        await Use(player, "arrow");
                    }

                    player.StaminaCounter++;
                }

                await Sell(player);
                await SendGold(player);

                //When account have more than 1 character then players.Count also always will be more than 1, even when rest of characters are paused.
                player.Status = false;
                player.StatusInfo = "Przerwa";
                if (players.Count != 1)
                {
                    await Wait(7 * 100, 12 * 100);
                }
            }

            return;
        }
        #endregion
    }
}
