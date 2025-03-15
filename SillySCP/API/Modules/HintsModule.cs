using Exiled.API.Features;
using Exiled.API.Features.Roles;
using RueI;
using RueI.Displays;
using RueI.Displays.Scheduling;
using RueI.Elements;
using RueI.Extensions;
using SillySCP.API.Extensions;
using SillySCP.API.Features;

namespace SillySCP.API.Modules
{
    public static class HintsModule
    {
        // kills manager
        
        public static DynamicElement KillsElement = new (GetKills, 250);
        
        public static string GetKills(DisplayCore core)
        {
            if (!Player.TryGet(core.Hub, out Player player))
                return string.Empty;

            if (player.Role is not SpectatorRole spectatorRole) return string.Empty;
            
            Player spectating = spectatorRole.SpectatedPlayer;
            
            if(spectating == null) return string.Empty;
            
            if(spectating.DoNotTrack) return "Kill Count: Unknown";
            
            PlayerStat spectatingPlayerStat = spectating.FindPlayerStat();
            
            if (spectatingPlayerStat == null) return "Kill Count: 0";
            
            if(spectatingPlayerStat.Player.IsScp) return $"Kill Count: {spectatingPlayerStat.ScpKills ?? 0}";
            
            return $"Kill Count: {spectatingPlayerStat.Kills}";
        }
        
        // timed hints manager

        public static void ShowString(this ReferenceHub hub, string text, float duration = 3f, float? position = null)
        {
            float pos = position ?? Ruetility.FunctionalToScaledPosition(0);
            
            DisplayCore.Get(hub).SetElemTemp(text, pos, TimeSpan.FromSeconds(duration), new ());
        }

        public static void ShowString(this Player player, string text, float duration = 3f, float? position = null)
        {
            player.ReferenceHub.ShowString(text, duration, position);
        }
    }
}