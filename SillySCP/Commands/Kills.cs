using System;
using CommandSystem;
using RemoteAdmin;
using Player = PluginAPI.Core.Player;

namespace SillySCP.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class Kills : ICommand
    {
        public string Command { get; } = "kills";

        public string[] Aliases { get; } = new string[] { "k" };

        public string Description { get; } = "Get the amount of kills you have.";

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

            if (player.DoNotTrack)
            {
                response =
                    "You have do not track enabled, disable it so we can start tracking your kills!";
                return false;
            }

            var playerStat = Plugin.Instance.PlayerStats.Find((p) => p.Player == player);
            if (playerStat == null)
            {
                response = "You have 0 kills!";
                return true;
            }

            response = "You have " + (playerStat.Kills ?? 0) + " player kills and " + (playerStat.ScpKills ?? 0) + " SCP kills!";
            return true;
        }
    }
}
