using Exiled.API.Features;
using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerRoles;
using Respawning;
using Respawning.Waves;
using SillySCP.API.Extensions;
using SillySCP.API.Features;
using SillySCP.API.Interfaces;

namespace SillySCP.Handlers
{
    public class RespawnSystemHandler : IRegisterable
    {
        public static RespawnSystemHandler Instance { get; private set; }
        
        public TimeBasedWave NtfWave1 { get; private set; }
        public TimeBasedWave NtfWave2 { get; private set; }
        
        public TimeBasedWave ChaosWave1 { get; private set; }
        public TimeBasedWave ChaosWave2 { get; private set; }
        
        public string SpectatingTimerText
        {
            get
            {
                TimeSpan ntfTime = NtfRespawnTime();
                TimeSpan chaosTime = ChaosRespawnTime();
                return $"<voffset=32em>{ntfTime.Minutes:D1}<size=22>M</size> {ntfTime.Seconds:D2}<size=22>S</size><space=16em>{chaosTime.Minutes:D1}<size=22>M</size> {chaosTime.Seconds:D2}<size=22>S</size></voffset>";
            }
        }
        public string NormalTimerText
        {
            get
            {
                TimeSpan ntfTime = NtfRespawnTime();
                TimeSpan chaosTime = ChaosRespawnTime();
                return $"<voffset=34em>{ntfTime.Minutes:D1}<size=22>M</size> {ntfTime.Seconds:D2}<size=22>S</size><space=16em>{chaosTime.Minutes:D1}<size=22>M</size> {chaosTime.Seconds:D2}<size=22>S</size></voffset>";
            }
        }

        public void Init()
        {
            Instance = this;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
        }
        
        public void Unregister()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
        }
        
        private void OnRoundStarted()
        {
            foreach (SpawnableWaveBase wave in WaveManager.Waves)
            {
                if(wave is not TimeBasedWave timedWave) continue;
                if (timedWave.TargetFaction == Faction.FoundationStaff)
                {
                    if(NtfWave1 == null) NtfWave1 = timedWave;
                    else NtfWave2 = timedWave;
                }
                else
                {
                    if(ChaosWave1 == null) ChaosWave1 = timedWave;
                    else ChaosWave2 = timedWave;
                }
            }
        }

        private void OnRoundEnded(RoundEndedEventArgs ev)
        {
            NtfWave1 = null;
            NtfWave2 = null;
            ChaosWave1 = null;
            ChaosWave2 = null;
        }

        private IEnumerator<float> RespawnTimer()
        {
            while (Round.InProgress)
            {
                string timerText = NormalTimerText;
                string spectatingText = SpectatingTimerText;
                foreach (Exiled.API.Features.Player player in Exiled.API.Features.Player.List)
                {
                    if (player.Role != RoleTypeId.Spectator) continue;
                    
                    PlayerStat playerStat = player.FindPlayerStat();
                    if (playerStat == null) continue;
                    PlayerStat spectatingPlayerStat = playerStat.Spectating;
                    string kills = ((spectatingPlayerStat != null ? spectatingPlayerStat.Player.IsScp ? spectatingPlayerStat.ScpKills : spectatingPlayerStat.Kills : 0) ?? 0).ToString();
                    string spectatingKills =
                        spectatingPlayerStat != null
                            ? spectatingPlayerStat.Player.DoNotTrack == false ? string.IsNullOrEmpty(kills) ? "Unknown" : kills : "Unknown"
                            : "0";

                    string timersText = playerStat.Spectating != null
                        ? spectatingText
                        : timerText;
                    
                    string killsText = playerStat.Spectating != null ? "\n\nKill count: " + spectatingKills : "";
                
                    string text = timersText + killsText;
                    text = text.Trim();
                
                    player.ShowHint(text, 2f);
                    yield return Timing.WaitForSeconds(1f);
                }
            }

            yield return 0;
        }

        public TimeSpan NtfRespawnTime()
        {
            if(NtfWave1 != null && !NtfWave1.Timer.IsPaused) 
                return TimeSpan.FromSeconds(NtfWave1.Timer.TimeLeft);
            else if (NtfWave2 != null && !NtfWave2.Timer.IsPaused)
                return TimeSpan.FromSeconds(NtfWave2.Timer.TimeLeft);
            else 
                return TimeSpan.Zero;
        }
        
        public TimeSpan ChaosRespawnTime()
        {
            if(ChaosWave1 != null && !ChaosWave1.Timer.IsPaused) 
                return TimeSpan.FromSeconds(ChaosWave1.Timer.TimeLeft);
            else if (ChaosWave2 != null && !ChaosWave2.Timer.IsPaused)
                return TimeSpan.FromSeconds(ChaosWave2.Timer.TimeLeft);
            else 
                return TimeSpan.Zero;
        }
    }
}