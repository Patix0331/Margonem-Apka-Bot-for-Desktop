using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ApkaBotLibrary.Account;
using ApkaBotLibrary.Requests.ResponseInfo;
using Newtonsoft.Json;

namespace ApkaBotLibrary.Requests
{
    public partial class Request
    {
        private async Task GetReward(Character player)
        {
            string[] dialogs = new string[]
            {
                "Wyjmuję",
                "Ze skrzyni wyjmujesz kilka",
                "Systematycznie sprawdzaj"
            };
            _cookies.Add(new Uri($"http://{player.World}.margonem.pl/"), new Cookie("mchar_id", player.h.id));
            _cookies.Add(new Uri($"http://{player.World}.margonem.pl/"), new Cookie("interface", "ni"));

            await ClientInit(player);
            await Wait();

            var responseDialogs = new List<string>();//await NpcTalk(player, 148890);

            foreach (var dialog in dialogs)
            {
                Start:
                for (int i = 0; i < responseDialogs.Count; i++)
                {
                    if (responseDialogs[i].Contains(dialog))
                    {
                        responseDialogs = await NpcTalk(player, 148890, responseDialogs[i + 1]);
                        await Wait(600, 800);
                        goto Start;
                    }
                }
            }

            await ClientInit(player);
            await Wait();

            await Get(player, $"moveitem&st=1&id={player.Items.FirstOrDefault(x => x.Value.name.Contains("Szkatułka")).Key}", mobile: false);
            await Wait();

            string[] movements = new string[] {
                "50,33", "51,33", "52,33", "52,34", "52,35", "52,36", "52,37", "52,39", "52,40", "53,40", "54,40", "55,40", "56,40", "57,40"
            };

            foreach (var cords in movements)
            {
                await Get(player, $"_&ml={cords}&mts={new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()+0.4}", mobile: false);
                await Wait(400,500);
            }

            dialogs = new string[]
            {
                "ja niczego",
                "Czy w takim",
                "Co radzisz",
                "Wyruszam na"
            };

            responseDialogs = await NpcTalk(player, 142715);

            foreach (var dialog in dialogs)
            {
                Start:
                for (int i = 0; i < responseDialogs.Count; i++)
                {
                    if (dialogs[i].Contains(dialog))
                    {
                        responseDialogs = await NpcTalk(player, player.GetSettings.ClientShop.NpcId, dialogs[++i]);
                        await Wait(600, 800);
                        goto Start;
                    }
                }
            }

            responseDialogs = await NpcTalk(player, 142715);

            string response = "";
            for (int i = 0; i < responseDialogs.Count; i++)
            {
                if (dialogs[i].Contains("Chciałbym"))
                {
                    await Wait();
                    response = await getRequest(player, $"talk&id=142715&c={responseDialogs[i + 1]}");
                }
            }

            var json = new
            {
                item = new Dictionary<string, Item>()
            };

            json = JsonConvert.DeserializeAnonymousType(response, json);

            string[] items = new string[]
            {
                "Różdżka druida",
                "Orb druida",
                "Płaszcz czarnej magii",
                "Rękawice sprytu",
                "Stalowe buty",
                "Wzmocniony stalowy hełm",
                "Naszyjnik sprawności",
                "Pierścień sprawności"
            };

            StringBuilder toBuy = new StringBuilder();
            foreach (var buy in items)
            {
                foreach (var item in json.item)
                {
                    if (item.Value.name == buy)
                    {
                        toBuy.Append($"{item.Key},1");
                        if (item.Value.name != "Pierścień sprawności")
                        {
                            toBuy.Append(';');
                        }
                    }
                }
            }

            await Wait(400, 600);
            await Get(player, $"shop&buy={toBuy}&sell=", mobile: false);
            await Wait();
            await ClientInit(player);
            await Wait();

            foreach (var item in items)
            {
                var toEquip = player.Items.FirstOrDefault(x => x.Value.name == item).Key;
                await Get(player, $"moveitem&st=1&id={toEquip}", mobile: false);
                await Wait(400, 500);
            }
        }
    }
}
