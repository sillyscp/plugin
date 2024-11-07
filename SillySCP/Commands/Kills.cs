using System;
using CommandSystem;
using Exiled.API.Features;

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
            Player.TryGet(sender, out var player);

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
