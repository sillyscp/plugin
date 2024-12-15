using System.Drawing;
using Exiled.Events.EventArgs.Server;
using PlayerRoles;
using Respawning;
using Respawning.Waves;
using RueI.Displays;
using RueI.Elements;
using SillySCP.API.Interfaces;
using StringBuilder = System.Text.StringBuilder;
using RueI.Extensions.HintBuilding;
using RueI.Parsing.Enums;

namespace SillySCP.Handlers
{
    public class RespawnSystemHandler : IRegisterable
    {
        public static RespawnSystemHandler Instance { get; private set; }
        
        public TimeBasedWave NtfWave1 { get; private set; }
        public TimeBasedWave NtfWave2 { get; private set; }
        
        public TimeBasedWave ChaosWave1 { get; private set; }
        public TimeBasedWave ChaosWave2 { get; private set; }
        
        public AutoElement RespawnTimerDisplay { get; private set; }

        public void Init()
        {
            Instance = this;
            
            RespawnTimerDisplay = new(Roles.Spectator, new DynamicElement(GetTimers, 910))
            {
                UpdateEvery = new (TimeSpan.FromSeconds(1))
            };
            
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
        }
        
        public void Unregister()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
        }

        private string GetTimers(DisplayCore core)
        {
            TimeSpan ntfTime = NtfRespawnTime();
            TimeSpan chaosTime = ChaosRespawnTime();

            StringBuilder builder = new StringBuilder()
                .SetAlignment(HintBuilding.AlignStyle.Center);
            
            builder
                .Append(ntfTime.Minutes.ToString("D1"))
                .SetSize(22)
                .Append("M")
                .CloseSize()
                .Append(" ")
                .Append(ntfTime.Seconds.ToString("D2"))
                .SetSize(22)
                .Append("S")
                .CloseSize();
            
            builder
                .AddSpace(16, MeasurementUnit.Ems);
            
            builder
                .Append(chaosTime.Minutes.ToString("D1"))
                .SetSize(22)
                .Append("M")
                .CloseSize()
                .Append(" ")
                .Append(chaosTime.Seconds.ToString("D2"))
                .SetSize(22)
                .Append("S")
                .CloseSize();
            
            return builder.ToString();
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

        public TimeSpan NtfRespawnTime()
        {
            if(NtfWave1 != null && !NtfWave1.Timer.IsPaused) 
                return TimeSpan.FromSeconds(NtfWave1.Timer.TimeLeft + 18);
            else if (NtfWave2 != null && !NtfWave2.Timer.IsPaused)
                return TimeSpan.FromSeconds(NtfWave2.Timer.TimeLeft + 18);
            else if (NtfWave1 != null && NtfWave1.Timer.IsPaused)
                return TimeSpan.FromSeconds(NtfWave1.Timer.TimeLeft + 18);
            else if (NtfWave2 != null && NtfWave2.Timer.IsPaused)
                return TimeSpan.FromSeconds(NtfWave2.Timer.TimeLeft + 18);
            else
                return TimeSpan.Zero;
        }
        
        public TimeSpan ChaosRespawnTime()
        {
            if(ChaosWave1 != null && !ChaosWave1.Timer.IsPaused) 
                return TimeSpan.FromSeconds(ChaosWave1.Timer.TimeLeft + 13);
            else if (ChaosWave2 != null && !ChaosWave2.Timer.IsPaused)
                return TimeSpan.FromSeconds(ChaosWave2.Timer.TimeLeft + 13);
            else if (ChaosWave1 != null && ChaosWave1.Timer.IsPaused)
                return TimeSpan.FromSeconds(ChaosWave1.Timer.TimeLeft + 13);
            else if (ChaosWave2 != null && ChaosWave2.Timer.IsPaused)
                return TimeSpan.FromSeconds(ChaosWave2.Timer.TimeLeft + 13);
            else 
                return TimeSpan.Zero;
        }
    }
}