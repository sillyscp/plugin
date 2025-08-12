using LabApi.Features.Wrappers;
using PlayerRoles.Spectating;
using RueI;
using RueI.Displays;
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
            Player player = Player.Get(core.Hub);
            
            if(player.IsAlive) return string.Empty;
            
            Player spectating = player.CurrentlySpectating;
            
            if(spectating == null) return string.Empty;
            
            if(spectating.DoNotTrack) return "Kill Count: Unknown";
            
            PlayerStat spectatingPlayerStat = spectating.FindPlayerStat();
            
            if (spectatingPlayerStat == null) return "Kill Count: 0";
            
            if(spectatingPlayerStat.Player.IsSCP) return $"Kill Count: {spectatingPlayerStat.ScpKills ?? 0}";
            
            return $"Kill Count: {spectatingPlayerStat.Kills}";
        }
        
        // timed hints manager

        public static void ShowString(this ReferenceHub hub, string text, float duration = 3f, float? position = null)
        {
            
            DisplayCore.Get(hub).SetElemTemp(text, position ?? 300, TimeSpan.FromSeconds(duration), new ());
        }

        public static void ShowString(this Player player, string text, float duration = 3f, float? position = null)
        {
            player.ReferenceHub.ShowString(text, duration, position);
        }
    }
}