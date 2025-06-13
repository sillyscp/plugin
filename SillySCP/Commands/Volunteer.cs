using CommandSystem;
using LabApi.Features.Extensions;
using LabApi.Features.Wrappers;
using PlayerRoles;
using SillySCP.API.Features;

namespace SillySCP.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class Volunteer: ICommand
    {
        public string Command { get; } = "volunteer";

        public string[] Aliases { get; } = new [] { "vol", "v" };

        public string Description { get; } = "Volunteer to become an SCP.";

        public bool Execute(
            ArraySegment<string> arguments,
            ICommandSender sender,
            out string response
        )
        {
            Player player = Player.Get(sender);
            if (player == null)
            {
                response = "Only players can use this command!";
                return false;
            }

            if (player.IsSCP && player.Role != RoleTypeId.Scp0492) // zombies should beable to volunteer for regular scp's 
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
            
            if (!VolunteerSystem.VaildScps.TryGetValue(arguments.At(0), out role) && !Enum.TryParse(arguments.At(0), true, out role))
            {
                response = "Error parsing the RoleTypeId.";
                return false;
            }
            if (player.IsAlive && role == RoleTypeId.Scp0492)
            {
                response = "Alive Players cannot volunteer as a zombie";
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
            
            player.SendBroadcast("You have volunteered to become " + role.GetFullName() + "!", 5, Broadcast.BroadcastFlags.Normal, true);
            
            VolunteerSystem.RaiseVolunteerEvent(player, volunteer);
            
            response = "Done!";
            return true;
        }
    }
}