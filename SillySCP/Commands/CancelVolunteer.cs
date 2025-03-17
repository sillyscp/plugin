using CommandSystem;
using Exiled.API.Extensions;
using Exiled.API.Features;
using PlayerRoles;
using SillySCP.API.Features;

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

            response = "Canceling " + role.GetFullName();
            if (VolunteerSystem.Volunteers.Count < 1)
            {
                response = "You do not have any volunteers yet";
                return false;
            }

            var volunteer = VolunteerSystem.Volunteers.Find(v => v.Replacement == role);

            Map.Broadcast(5, $"Volunteer {volunteer.Replacement.GetFullName()} was revoked", shouldClearPrevious: true);
            VolunteerSystem.Volunteers.Remove(volunteer);
            response = "Canceling " + role.GetFullName();
            return true;
        }
    }
}
