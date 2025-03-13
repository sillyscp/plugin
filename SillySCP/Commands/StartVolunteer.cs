using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using MEC;
using PlayerRoles;
using PlayerRoles.RoleAssign;
using SillySCP.API.Features;

namespace SillySCP.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class StartVolunteer : ICommand
    {
        public string Command { get; } = "startvolunteer";
        public string Description { get; } = "Start a volunteer prompt";
        public string[] Aliases { get; } = new[] { "sv", "svol" };

        private void CreateVolunteer(RoleTypeId oldRole)
        {
            Volunteers volunteer = new ()
            {
                Replacement = oldRole,
                Players = new()
            };
            VolunteerSystem.Volunteers ??= new();
            VolunteerSystem.Volunteers.Add(volunteer);
            Map.Broadcast(10,
                $"{oldRole.GetFullName()} has left the game\nPlease run .volunteer {(oldRole == RoleTypeId.Scp0492 ? "zombie" : oldRole.GetFullName().Split('-')[1])} to volunteer to be the SCP");
            Timing.RunCoroutine(VolunteerSystem.ChooseVolunteers(volunteer));
        }
        private Dictionary<string, RoleTypeId> VaildScps { get; set; } = new Dictionary<string, RoleTypeId>
        {
            { "173", RoleTypeId.Scp173 },
            { "peanut", RoleTypeId.Scp173 },
            { "939", RoleTypeId.Scp939 },
            { "079", RoleTypeId.Scp079 },
            { "79", RoleTypeId.Scp079 },
            { "computer", RoleTypeId.Scp079 },
            { "106", RoleTypeId.Scp106 },
            { "larry", RoleTypeId.Scp106 },
            { "096", RoleTypeId.Scp096 },
            { "96", RoleTypeId.Scp096 },
            { "shyguy", RoleTypeId.Scp096 },
            { "049", RoleTypeId.Scp049 },
            { "49", RoleTypeId.Scp049 },
            { "doctor", RoleTypeId.Scp049 },
            { "0492", RoleTypeId.Scp0492 },
            { "492", RoleTypeId.Scp0492 },
            { "zombie", RoleTypeId.Scp0492 },
        };
        
        public bool Execute(
            ArraySegment<string> arguments,
            ICommandSender sender,
            out string response
        )
        {
            if (arguments.Count != 1)
            {
                response = "Usage: startvolunteer <SCP>";
                return false;
            }
            if (!VaildScps.TryGetValue(arguments.At(0), out RoleTypeId scp))
            {
                response = "Must be a SCP";
                return false;
            }
            
            response = $"Starting volunteer {scp}";
            CreateVolunteer(scp);
            return true;
        }
    }
}