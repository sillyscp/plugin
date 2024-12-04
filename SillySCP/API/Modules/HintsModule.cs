using RueI.Displays;
using RueI.Elements;
using SillySCP.API.Extensions;
using SillySCP.API.Features;

namespace SillySCP.API.Modules
{
    public static class HintsModule
    {
        public static DynamicElement KillsElement = new (GetKills, 250);
        
        public static string GetKills(DisplayCore core)
        {
            if (!Exiled.API.Features.Player.TryGet(core.Hub, out Exiled.API.Features.Player player))
                return string.Empty;
            
            PlayerStat playerStat = player.FindPlayerStat();
            
            if(playerStat == null || playerStat.Spectating == null) return string.Empty;
            
            if(playerStat.Spectating.DoNotTrack) return "Kill Count: Unknown";
            
            PlayerStat spectatingPlayerStat = playerStat.Spectating.FindPlayerStat();
            
            if (spectatingPlayerStat == null) return "Kill Count: 0";
            
            if(spectatingPlayerStat.Player.IsScp) return $"Kill Count: {spectatingPlayerStat.ScpKills ?? 0}";
            
            return $"Kill Count: {spectatingPlayerStat.Kills}";
        }
    }
}