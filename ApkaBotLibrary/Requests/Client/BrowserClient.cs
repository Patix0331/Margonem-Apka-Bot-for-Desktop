using ApkaBotLibrary.Account;
using ApkaBotLibrary.Requests.ResponseInfo;
using ApkaBotLibrary.Utils.Hash;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ApkaBotLibrary.Requests
{
    public partial class Request
    {
        private async Task ClientInit(Character player)
        {
            for (int level = 1; level < 5; level++)
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(string
                        .Concat(new string[] {
                                    $"http://{player.World}",
                                    ".margonem.pl/engine?t=init&initlvl=",
                                    $"{level}",
                                    $"&clientTs={new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()}",
                                    $"&mucka={Mucka.GenerateMucka()}",
                                    player.BrowserToken == null ? "" : $"&browser_token={player.BrowserToken}"
                                    })),
                };
                request.Headers.Referrer = new Uri($"http://{player.World}.margonem.pl/");

                var result = await _apiClient.SendAsync(request);

                string response = await result.Content.ReadAsStringAsync(); 

                if (response.Contains("Nie jesteś zalogowany"))
                {
                    Environment.Exit(1);
                }

                if (response.Contains("Przerwa techniczna - aktualizacja silnika."))
                {
                    player.StatusInfo = "Przerwa techniczna";
                    await Wait(60 * 1000, 60 * 2000);
                    await ClientInit(player);
                    return;
                }

                if (response.Contains("wait_for"))
                {
                    await Task.Delay(5000);
                    await ClientInit(player);
                    return;
                }

                var BrowserToken = new
                {
                    browser_token = ""
                };

                BrowserToken = JsonConvert.DeserializeAnonymousType(response, BrowserToken);

                if (level == 1)
                {
                    player.BrowserToken = BrowserToken.browser_token;
                }
                if (level == 3)
                {
                    var Items = new
                    {
                        item = new Dictionary<string, Item>()
                    };

                    Items = JsonConvert.DeserializeAnonymousType(response, Items);
                    player.Items = Items.item;
                }
            }
        }

        private async Task<List<string>> NpcTalk(Character player, int npcID, string message = null)
        {
            var json = new
            {
                d = new List<string>(),
                shop = new
                {
                    purchase = ""
                }
            };
            var action = message is null ? $"talk&id={npcID}" : $"talk&id={npcID}&c={message}";

            var response = await getRequest(player, action, false, false);
            json = JsonConvert.DeserializeAnonymousType(response, json);

            if (message is null)
            {
                return json.d;
            }

            return new List<string>(JsonConvert.DeserializeAnonymousType(response, json).shop.purchase.Split(','));
        }

        private async Task MainClientMethod(Character player)
        {
            if (player.ToSell.Count <= 0)
            {
                return;
            }
            _cookies.Add(new Uri($"http://{player.World}.margonem.pl/"), new Cookie("mchar_id", player.h.id));
            _cookies.Add(new Uri($"http://{player.World}.margonem.pl/"), new Cookie("interface", "ni"));

            await ClientInit(player);

            var dialogs = await NpcTalk(player, player.GetSettings.ClientShop.NpcId);

            List<string> types = new List<string>();

            for (int i = 0; i < dialogs.Count; i++)
            {
                if (dialogs[i].Contains(player.GetSettings.ClientShop.ShopMessage))
                {
                    types = await NpcTalk(player, player.GetSettings.ClientShop.NpcId, dialogs[++i]);
                }
            }

            Dictionary<string, Item> items = new Dictionary<string, Item>();
            foreach (var item in player.ToSell)
            {
                foreach (var cl in types)
                {
                    if (item.Value.cl == cl)
                    {
                        items.Add(item.Key, item.Value);
                        player.SoldItems.Add(DateTime.Now, item.Value);
                        player.ToSell.Remove(item.Key);
                        break;
                    }
                }
            }

            StringBuilder toSell = new StringBuilder();
            toSell.AppendJoin(",", items.Keys);
            player.StatusInfo = $"Sprzedaję {items.Count} itemków u npc";

            await Wait(1000,1100);
            await Get(player, $"shop&buy=&sell={toSell}", mobile: false);

            await Init(player);
        }
    }
}
