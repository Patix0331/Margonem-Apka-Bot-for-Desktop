using ApkaBotLibrary.Account;
using System.Threading.Tasks;

namespace ApkaBotLibrary.Requests.SendGold
{
    public enum Methods
    {
        Clan = 1,
        Player
    }

    public class SendGold
    {
        public Methods Method { get; set; }
        public string Receiver { get; set; }
        public string message { get; set; }
    }
}
namespace ApkaBotLibrary.Requests
{
    public partial class Request
    {
        private async Task SendGold(Character player)
        {
            string action;
            ulong earnedGold = player.h.gold - player.StartGold;
            if (earnedGold == 0)
            {
                return;
            }
            switch (player.GetSettings.SendGold.Method)
            {
                case Requests.SendGold.Methods.Clan:
                    if (player.h.clan == null)
                    {
                        return;
                    }
                    player.StatusInfo = $"Wysyłam złoto do klanu";
                    action = $"clan&a=save&f=gold&v={earnedGold}";
                    break;
                case Requests.SendGold.Methods.Player:
                    if (player.GetSettings.SendGold.Receiver == null)
                    {
                        return;
                    }
                    player.StatusInfo = $"Wysyłam złoto do {player.GetSettings.SendGold.Receiver}";
                    action = $"mail&a=send&r={player.GetSettings.SendGold.Receiver}&gold={earnedGold}&iid=&msg={player.GetSettings.SendGold.message}";
                    break;
                default:
                    return;
            }
            await Wait();
            await Get(player, action);
            await Wait();
        }
    }
}