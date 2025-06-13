using CommandSystem;
using LabApi.Features.Extensions;
using LabApi.Features.Wrappers;
using PlayerRoles;
using SillySCP.API.Features;
using Utils.NonAllocLINQ;

namespace SillySCP.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class CancelVolunteer : ICommand
    {
        public string Command { get; } = "cancelvolunteer";
        public string Description { get; } = "Cancel a volunteer prompt";
        public string[] Aliases { get; } = new[] { "cv", "rv", "rvol", "cvol","revokevolunteer"};

        public bool Execute(
            ArraySegment<string> arguments,
            ICommandSender sender,
            out string response
        )

        {
            if (arguments.Count != 1)
            {
                response = "Usage: cancelvolunteer <SCP>";
                return false;
            }

            if (!VolunteerSystem.VaildScps.TryGetValue(arguments.At(0), out RoleTypeId role))
            {
                response = "Must be a SCP";
                return false;
            }

            if (VolunteerSystem.Volunteers.Count < 1)
            {
                response = "No Active Volunteers";
                return false;
            }

            if (!VolunteerSystem.Volunteers.TryGetFirst(v => v.Replacement == role,out Volunteers volunteer))
            {
                response = "Cant find this volunteer";
                return false;
            }

            Server.SendBroadcast($"Volunteer {volunteer.Replacement.GetFullName()} was revoked", 5, shouldClearPrevious: true);
            VolunteerSystem.Volunteers.Remove(volunteer);
            
            response = "Canceling " + role.GetFullName();
            return true;
        }
    }
}
