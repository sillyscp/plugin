using Exiled.API.Features;
using PlayerRoles;

namespace SillySCP.API.Features
{
    #nullable enable
    public class Volunteers
    {
        public RoleTypeId Replacement;
        public Player? OriginalPlayer;
        public required List<Player> Players;
    }
}