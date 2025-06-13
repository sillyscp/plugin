using CommandSystem;
using LabApi.Features.Extensions;
using PlayerRoles;
using SillySCP.API.Features;

namespace SillySCP.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class StartVolunteer : ICommand
    {
        public string Command { get; } = "startvolunteer";
        public string Description { get; } = "Start a volunteer prompt";
        public string[] Aliases { get; } = new[] { "sv", "svol" };
        
        
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
            if (!VolunteerSystem.VaildScps.TryGetValue(arguments.At(0), out RoleTypeId scp))
            {
                response = "Must be a SCP";
                return false;
            }
            
            response = $"Starting volunteer {scp.GetFullName()}";
            VolunteerSystem.NewVolunteer(scp);
            return true;
        }
    }
}