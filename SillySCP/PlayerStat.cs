using Exiled.API.Features;
using PlayerRoles;

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
