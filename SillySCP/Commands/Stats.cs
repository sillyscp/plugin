using CommandSystem;
using Exiled.API.Features;
using SillySCP.API.Features;

namespace SillySCP.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class Stats : ICommand
    {
        public string Command { get; } = "stats";

        public string[] Aliases { get; } = new [] { "k", "s", "kills" };

        public string Description { get; } = "Get your stats for this round.";

        public bool Execute(
            ArraySegment<string> arguments,
            ICommandSender sender,
            out string response
        )
        {
            Player.TryGet(sender, out Player player);

            if (player == null)
            {
                response = "Only players can use this command!";
                return false;
            }

            if (player.DoNotTrack)
            {
                response =
                    "You have do not track enabled, disable it so we can start tracking your stats!";
                return false;
            }

            PlayerStat playerStat = Plugin.Instance.PlayerStats.Find((p) => p.Player == player);
            if (playerStat == null)
            {
                response = "You have no stats to show!";
                return true;
            }

            response = "You have " + (playerStat.Kills ?? 0) + " player kills, " + (playerStat.ScpKills ?? 0) + " SCP kills, and " + (playerStat.Damage ?? 0) + " damage!";
            return true;
        }
    }
}
