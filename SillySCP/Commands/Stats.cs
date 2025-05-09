using CommandSystem;
using Exiled.API.Features;
using SillySCP.API.Features;

namespace SillySCP.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class Damage : ICommand
    {
        public string Command { get; } = "damage";
        public string[] Aliases { get; } = new [] { "d", "dam" };
        public string Description { get; } = "Get the amount of damage you've dealt.";

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
                    "You have do not track enabled, disable it so we can start tracking your damage!";
                return false;
            }

            if (player.IsAlive)
            {
                response = "You must be dead to run this command!";
                return false;
            }

            PlayerStat playerStat = Plugin.Instance.PlayerStats.Find((p) => p.Player == player);
            if (playerStat == null)
            {
                response = "You have dealt 0 damage!";
                return true;
            }

            response = "You have dealt " + Math.Floor(playerStat.Damage ?? 0) + " damage!";
            return true;
        }
    }
}
