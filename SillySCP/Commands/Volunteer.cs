using System;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using PlayerRoles;
using RemoteAdmin;
using Player = PluginAPI.Core.Player;

namespace SillySCP.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class Volunteer
    {
        public string Command { get; } = "volunteer";

        public string[] Aliases { get; } = new string[] { "vol" };

        public string Description { get; } = "Volunteer to become an SCP.";

        public bool Execute(
            ArraySegment<string> arguments,
            ICommandSender sender,
            out string response
        )
        {
            if (!(sender is PlayerCommandSender playerSender))
            {
                response = "Only players can use this command!";
                return false;
            }

            var player = Player.GetPlayers().Find((p) => p.PlayerId == playerSender.PlayerId);

            if (player == null)
            {
                response = "Only players can use this command!";
                return false;
            }
            
            if(arguments.Count != 1)
            {
                response = "Usage: .volunteer <role>";
                return false;
            }

            if (!Enum.TryParse($"Scp{arguments.First()}", out RoleTypeId role))
            {
                response = "Invalid role!";
                return false;
            }

            var volunteer = Plugin.Instance.Volunteers.FirstOrDefault((v) => v.Replacement == role);
            
            if (volunteer == null)
            {
                response = "Invalid role!";
                return false;
            }
            
            if (volunteer.Players.Contains(player))
            {
                response = "You are already a volunteer!";
                return false;
            }
            
            volunteer.Players.Add(player);
            
            player.SendBroadcast("You have volunteered to become SCP-" + arguments.First() + "!", 5, Broadcast.BroadcastFlags.Normal, true);
            
            response = "Done!";
            return true;
        }
    }
}