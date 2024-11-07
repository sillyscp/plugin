using Exiled.API.Features;

namespace SillySCP
{
    public class PlayerStat
    {
        public Player Player { get; set; }
        public int? Kills { get; set; }
        public Player Spectating { get; set; }
        public int? ScpKills { get; set; }
    }
}
