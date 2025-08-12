using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using LabApi.Features.Wrappers;
using MEC;
using SillySCP.API.Modules;

namespace SillySCP.Commands.ScpSwap
{
    public class Deny : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!Player.TryGet(sender, out Player player))
            {
                response = "No player found";
                return false;
            }

            if (!ScpSwap.AwaitingRequests.ContainsValue(player))
            {
                response = "No requests found";
                return false;
            }

            Player requester = ScpSwap.AwaitingRequests.FirstOrDefault(x => x.Value == player).Key;

            ScpSwap.AwaitingRequests.Remove(requester);
            
            if (ScpSwap.Handles.TryGetValue(requester, out CoroutineHandle handle))
            {
                Timing.KillCoroutines(handle);
                ScpSwap.Handles.Remove(requester);
            }
            
            requester.ShowString($"Your request to switch to {player.RoleBase.RoleName} got denied.", 10);

            response = "Successfully denied the request";
            return true;
        }

        public string Command { get; } = "deny";
        public string[] Aliases { get; } = ["no", "n"];
        public string Description { get; } = "Deny an SCP Swap request";
    }
}