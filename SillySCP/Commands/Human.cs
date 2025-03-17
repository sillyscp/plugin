using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using PlayerRoles;
using PlayerRoles.RoleAssign;
using SillySCP.API.Features;

namespace SillySCP.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class Human : ICommand
    {
        public string Command { get; } = "human";

        public string[] Aliases { get; } = { "h" };

        public string Description { get; } = "Change into a human, if you're an SCP.";

        public bool Execute(
            ArraySegment<string> arguments,
            ICommandSender sender,
            out string response
        )
        {
            if (!Player.TryGet(sender, out Player player))
            {
                response = "Only Players can use this command.";
                return false;
            }
            
            if (player.Role == RoleTypeId.Scp0492)
            {
                response = "You cannot change into a human as SCP-049-2!, try .r or .requestVolunteer to request a spectator to take your place";
                return false;
            }
            
            if (!VolunteerSystem.ReadyVolunteers)
            {
                response = "You can not change into a human after the volunteer period is over!";
                return false;
            }
            
            
            if (!player.IsScp)
            {
                response = "Only SCPs can use this command!";
                return false;
            }
            
            RoleTypeId role = HumanSpawner.NextHumanRoleToSpawn;
            VolunteerSystem.NewVolunteer(player.Role);
            player.Role.Set(role, SpawnReason.None);
            response = $"You have been changed into {role.GetFullName()}!";
            return true;
        }
    }
}