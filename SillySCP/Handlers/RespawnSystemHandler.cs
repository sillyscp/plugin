using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerRoles;
using PluginAPI.Core;
using Respawning;
using Respawning.Waves;
using SillySCP.API.Interfaces;

namespace SillySCP.Handlers
{
    public class RespawnSystemHandler : IRegisterable
    {
        public static RespawnSystemHandler Instance { get; private set; }
        
        public TimeSpan NtfRespawnTime { get; private set; }
        public TimeSpan ChaosRespawnTime { get; private set; }
        
        public string SpectatingTimerText => $"<voffset=32em>{NtfRespawnTime.Minutes:D1}<size=22>M</size> {NtfRespawnTime.Seconds:D2}<size=22>S</size><space=16em>{ChaosRespawnTime.Minutes:D1}<size=22>M</size> {ChaosRespawnTime.Seconds:D2}<size=22>S</size></voffset>";
        public string NormalTimerText => $"<voffset=34em>{NtfRespawnTime.Minutes:D1}<size=22>M</size> {NtfRespawnTime.Seconds:D2}<size=22>S</size><space=16em>{ChaosRespawnTime.Minutes:D1}<size=22>M</size> {ChaosRespawnTime.Seconds:D2}<size=22>S</size></voffset>";
        
        private CoroutineHandle _timerCoroutine;

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
            _timerCoroutine = Timing.RunCoroutine(SetTimers());
        }

        private void OnRoundEnded(RoundEndedEventArgs ev)
        {
            Timing.KillCoroutines(_timerCoroutine);
        }

        private IEnumerator<float> SetTimers()
        {
            while (Exiled.API.Features.Round.InProgress)
            {
                yield return Timing.WaitForSeconds(1);
                TimeSpan ntfTime = TimeSpan.MaxValue;
                TimeSpan chaosTime = TimeSpan.MaxValue;

                TimeSpan lowestNtfTime = TimeSpan.MaxValue;
                TimeSpan lowestChaosTime = TimeSpan.MaxValue;

                foreach (SpawnableWaveBase wave in WaveManager.Waves)
                {
                    if (wave is not TimeBasedWave timedWave) continue;
                    TimeSpan timer = TimeSpan.FromSeconds(timedWave.Timer.TimeLeft);
                    if (timedWave.TargetFaction == Faction.FoundationStaff && ntfTime > timer &&
                        !timedWave.Timer.IsPaused)
                        ntfTime = timer;
                    else if (timedWave.TargetFaction == Faction.FoundationEnemy && chaosTime > timer &&
                             !timedWave.Timer.IsPaused)
                        chaosTime = timer;

                    if (timedWave.TargetFaction == Faction.FoundationStaff && lowestNtfTime > timer)
                        lowestNtfTime = timer;
                    else if (timedWave.TargetFaction == Faction.FoundationEnemy && lowestChaosTime > timer)
                        lowestChaosTime = timer;
                }

                if (ntfTime == TimeSpan.MaxValue) ntfTime = lowestNtfTime;
                if (chaosTime == TimeSpan.MaxValue) chaosTime = lowestChaosTime;
                
                if(ntfTime < TimeSpan.Zero) ntfTime = TimeSpan.Zero;
                if(chaosTime < TimeSpan.Zero) chaosTime = TimeSpan.Zero;

                NtfRespawnTime = ntfTime;
                ChaosRespawnTime = chaosTime;
            }

            yield return 0;
        }
    }
}