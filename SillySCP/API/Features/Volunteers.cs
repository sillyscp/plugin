using Exiled.API.Features;
using JetBrains.Annotations;
using PlayerRoles;

namespace SillySCP.API.Features
{
    public class Volunteers
    {
        public RoleTypeId Replacement;
        [CanBeNull] public Player OriginalPlayer;
        public List<Player> Players;
    }
}