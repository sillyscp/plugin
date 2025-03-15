using Exiled.API.Features;
using RueI.Displays;
using SillySCP.API.Modules;

namespace SillySCP.API.Features
{
    public class PlayerStat
    {
        public PlayerStat(Player player)
        {
            Player = player;

            SpectatingKillsDisplay = new(Player.ReferenceHub);
            SpectatingKillsDisplay.Elements.Add(HintsModule.KillsElement);
        }
        
        public Player Player { get; init; }
        public int? Kills { get; set; }
        public float? Damage { get; set; }
        
        public int? ScpKills { get; set; }
        public Display SpectatingKillsDisplay { get; set; }
    }
}
