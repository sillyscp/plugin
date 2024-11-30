using MEC;
using PlayerRoles;
using Respawning;
using Respawning.Waves;
using SillySCP.API.Extensions;
using SillySCP.Handlers;
using Player = Exiled.API.Features.Player;

namespace SillySCP.API.Features
{
    public static class RespawnSystem
    {
        public static IEnumerator<float> RespawnTimer(Player player)
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(1f);
                player = Player.List.FirstOrDefault(p => p.UserId == player.UserId);
                if (player == null || player.Role != RoleTypeId.Spectator)
                    break;
                
                PlayerStat playerStat = player.FindPlayerStat();
                PlayerStat spectatingPlayerStat = playerStat?.Spectating?.FindPlayerStat();
                string kills = ((spectatingPlayerStat != null ? spectatingPlayerStat.Player.IsScp ? spectatingPlayerStat.ScpKills : spectatingPlayerStat.Kills : 0) ?? 0).ToString();
                string spectatingKills =
                    spectatingPlayerStat != null
                        ? spectatingPlayerStat.Player.DoNotTrack == false ? string.IsNullOrEmpty(kills) ? "Unknown" : kills : "Unknown"
                        : "0";

                string timersText = $"<voffset=34em>{RespawnSystemHandler.Instance.NtfRespawnTime.Minutes:D1}<size=22>M</size> {RespawnSystemHandler.Instance.NtfRespawnTime.Seconds:D2}<size=22>S</size><space=16em>{RespawnSystemHandler.Instance.ChaosRespawnTime.Minutes:D1}<size=22>M</size> {RespawnSystemHandler.Instance.ChaosRespawnTime.Seconds:D2}<size=22>S</size></voffset>";
                
                string killsText = (playerStat?.Spectating != null ? "\n\nKill count: " + spectatingKills : "");
                
                string text = timersText + killsText;
                text = text.Trim();
                
                player.ShowHint(text, 2f);
            }
            
            yield return 0;
        }
    }
}