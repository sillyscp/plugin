using CommandSystem;
using Exiled.API.Features;

namespace SillySCP.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class TeleportPlayers : ICommand
    {
        public string Command { get; } = "teleportplayers";
        public string Description { get; } = "Teleport 1 player to another";
        public string[] Aliases { get; } = new[] { "tpp", "tpplayers" };

        public bool Execute(
            ArraySegment<string> arguments,
            ICommandSender sender,
            out string response
        )
        {
            if (arguments.Count != 2)
            {
                response = "Usage: teleportplayers <player1> <player2>";
                return false;
            }

            if (!int.TryParse(arguments.At(0), out int player1Id))
            {
                response = "Player 1 must be a number!";
                return false;
            }

            if (!int.TryParse(arguments.At(1), out int player2Id))
            {
                response = "Player 2 must be a number!";
                return false;
            }

            if (!Player.TryGet(player1Id, out Player player1))
            {
                response = "Player 1 not found!";
                return false;
            }

            if (!Player.TryGet(player2Id, out Player player2))
            {
                response = "Player 2 not found!";
                return false;
            }

            player1.Position = player2.Position;
            response = $"Teleported {player1.Nickname} to {player2.Nickname}";
            return true;
        }
    }
}