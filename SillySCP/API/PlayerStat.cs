using Exiled.API.Features;

namespace SillySCP.API
{
    public class PlayerStat
    {
        public Player Player { get; init; }
        public int? Kills { get; set; }
        public Player Spectating { get; set; }
        public int? ScpKills { get; set; }
    }
}
