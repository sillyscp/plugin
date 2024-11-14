using CommandSystem;
using PlayerRoles;
using SillySCP.API.Features;
using Player = Exiled.API.Features.Player;

namespace SillySCP.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class Volunteer: ICommand
    {
        public string Command { get; } = "volunteer";

        public string[] Aliases { get; } = new [] { "vol" };

        public string Description { get; } = "Volunteer to become an SCP.";

        public bool Execute(
            ArraySegment<string> arguments,
            ICommandSender sender,
            out string response
        )
        {
            if (!Player.TryGet(sender, out var player))
            {
                response = "Only players can use this command!";
                return false;
            }

            if (player == null)
            {
                response = "Only players can use this command!";
                return false;
            }

            if (player.IsScp)
            {
                response = "You are already an SCP! If they get replaced, you can swap with the person if they wish!";
                return false;
            }
            
            if(arguments.Count != 1)
            {
                response = "Usage: .volunteer <role>";
                return false;
            }

            RoleTypeId role;

            if (!Enum.TryParse("Scp" + arguments.At(0), true, out role) && !Enum.TryParse(arguments.At(0), true, out role))
            {
                response = "Error parsing the RoleTypeId.";
                return false;
            }

            Volunteers volunteer = VolunteerSystem.Volunteers.FirstOrDefault(v => v.Replacement == role);
            
            if (volunteer == null)
            {
                response = "Invalid role!";
                return false;
            }

            volunteer.Players ??= new ();
            
            if (volunteer.Players.Contains(player))
            {
                response = "You are already a volunteer!";
                return false;
            }
            
            volunteer.Players.Add(player);
            
            player.Broadcast(5, "You have volunteered to become SCP-" + arguments.First() + "!", Broadcast.BroadcastFlags.Normal, true);
            
            response = "Done!";
            return true;
        }
    }
}