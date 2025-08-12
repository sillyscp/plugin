using System.Diagnostics.CodeAnalysis;
using CommandSystem;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using SillySCP.API.Modules;
using UnityEngine;

namespace SillySCP.Commands.ScpSwap
{
    public class Accept : ICommand
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

            Player requester = ScpSwap.GetFromValue(player);

            Player toCheck = requester;

            while (ScpSwap.TryGetFromValue(toCheck, out Player replacer))
            {
                ScpSwapModule.Replace(replacer, toCheck);
                toCheck = replacer;
            }

            if (ScpSwap.Handles.TryGetValue(requester, out CoroutineHandle handle))
            {
                Timing.KillCoroutines(handle);
                ScpSwap.Handles.Remove(requester);
            }

            ScpSwapModule.Switch(player, requester);

            response = "Successfully accepted the request.";
            return true;
        }

        public string Command { get; } = "accept";
        public string[] Aliases { get; } = ["y", "yes"];
        public string Description { get; } = "Accept a SCP Swap request.";
    }
}