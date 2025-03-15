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

        public string[] Aliases { get; } = new [] { "h" };

        public string Description { get; } = "Change into a human, if you're an SCP.";

        public bool Execute(
            ArraySegment<string> arguments,
            ICommandSender sender,
            out string response
        )
        {
            if (!VolunteerSystem.ReadyVolunteers)
            {
                response = "You can not change into a human after the volunteer period is over!";
                return false;
            }
            
            Player.TryGet(sender, out Player player);
            
            if (!player.IsScp)
            {
                response = "Only SCPs can use this command!";
                return false;
            }

            if (player.Role.Type == RoleTypeId.Scp0492)
            {
                response = "You can not change into a human as SCP-049-2!";
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