using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using LabApi.Features.Wrappers;
using MEC;
using SillySCP.API.Modules;

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

            if (!ScpSwapModule.Cancel(player))
            {
                response = "You have no pending SCP Swap requests.";
                return false;
            }

            response = "Successfully cancelled your SCP Swap request.";
            return true;
        }

        public string Command { get; } = "cancel";
        public string[] Aliases { get; } = ["c"];
        public string Description { get; } = "Cancel an SCP Swap request.";
    }
}