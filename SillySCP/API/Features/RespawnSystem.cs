using Exiled.API.Features;
using MEC;
using PlayerRoles;
using Respawning;
using Respawning.Waves;
using SillySCP.API.Extensions;

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

                TimeSpan ntfTime = TimeSpan.MaxValue;
                TimeSpan chaosTime = TimeSpan.MaxValue;
                
                foreach (SpawnableWaveBase wave in WaveManager.Waves)
                {
                    if (wave is not TimeBasedWave timedWave) continue;
                    TimeSpan timer = TimeSpan.FromSeconds(timedWave.Timer.TimeLeft);
                    if (timedWave.TargetFaction == Faction.FoundationStaff && ntfTime > timer && !timedWave.Timer.IsPaused)
                        ntfTime = timer;
                    else if (timedWave.TargetFaction == Faction.FoundationEnemy && chaosTime > timer && !timedWave.Timer.IsPaused)
                        chaosTime = timer;
                }

                string timersText = "";
                
                if(ntfTime != TimeSpan.MaxValue && chaosTime != TimeSpan.MaxValue) 
                    timersText = $"<voffset=34em>{ntfTime.Minutes:D1}<size=22>M</size> {ntfTime.Seconds:D2}<size=22>S</size><space=16em>{chaosTime.Minutes:D1}<size=22>M</size> {chaosTime.Seconds:D2}<size=22>S</size></voffset>";
                
                string killsText = (playerStat?.Spectating != null ? "\n\nKill count: " + spectatingKills : "");
                
                string text = timersText + killsText;
                text = text.Trim();
                
                player.ShowHint(text, 2f);
            }
            
            yield return 0;
        }
    }
}