using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using LabApi.Features.Wrappers;
using MEC;

namespace SillySCP.Commands.ScpSwap
{
    public class Cancel : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!Player.TryGet(sender, out Player player))
            {
                response = "No player found";
                return false;
            }

            if (!ScpSwap.AwaitingRequests.Remove(player))
            {
                response = "You have no pending SCP Swap requests.";
                return false;
            }
            
            if (ScpSwap.Handles.TryGetValue(player, out CoroutineHandle handle))
            {
                Timing.KillCoroutines(handle);
                ScpSwap.Handles.Remove(player);
            }

            response = "Successfully cancelled your SCP Swap request.";
            return true;
        }

        public string Command { get; } = "cancel";
        public string[] Aliases { get; } = ["c"];
        public string Description { get; } = "Cancel an SCP Swap request.";
    }
}