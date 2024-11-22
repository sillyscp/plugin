using CommandSystem;
using Exiled.API.Extensions;
using Exiled.API.Features;
using PlayerRoles;
using PlayerRoles.RoleAssign;
using SillySCP.API.Features;

namespace SillySCP.Commands;

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
        }
        
        Player.TryGet(sender, out Player player);
        
        if (!player.IsScp)
        {
            response = "Only SCPs can use this command!";
            return false;
        }
        
        RoleTypeId role = HumanSpawner.NextHumanRoleToSpawn;
        
        player.Role.Set(role);
        response = $"You have been changed into {role.GetFullName()}!";
        return true;
    }
}