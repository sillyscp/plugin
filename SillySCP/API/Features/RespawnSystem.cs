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
        public static IEnumerator<float> RespawnTimer(PlayerStat player)
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(1f);
                if (player == null || player.Player.Role != RoleTypeId.Spectator)
                    break;
                
                PlayerStat spectatingPlayerStat = player.Spectating;
                string kills = ((spectatingPlayerStat != null ? spectatingPlayerStat.Player.IsScp ? spectatingPlayerStat.ScpKills : spectatingPlayerStat.Kills : 0) ?? 0).ToString();
                string spectatingKills =
                    spectatingPlayerStat != null
                        ? spectatingPlayerStat.Player.DoNotTrack == false ? string.IsNullOrEmpty(kills) ? "Unknown" : kills : "Unknown"
                        : "0";

                string timersText = player.Spectating != null
                    ? RespawnSystemHandler.Instance.SpectatingTimerText
                    : RespawnSystemHandler.Instance.NormalTimerText;
                
                string killsText = player.Spectating != null ? "\n\nKill count: " + spectatingKills : "";
                
                string text = timersText + killsText;
                text = text.Trim();
                
                player.Player.ShowHint(text, 2f);
            }
            
            yield return 0;
        }
    }
}