using Exiled.API.Features;

namespace SillySCP.API.Features
{
    public class PlayerStat
    {
        public Player Player { get; init; }
        public int? Kills { get; set; }
        public PlayerStat Spectating { get; set; }
        public int? ScpKills { get; set; }
    }
}
