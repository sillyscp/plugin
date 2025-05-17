using CommandSystem;
using Exiled.API.Features;

namespace SillySCP.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class LocalPosition : ICommand
    {
        public string Command { get; } = "LocalPosition";

        public string[] Aliases { get; } = [];

        public string Description { get; } = "";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            response = player.CurrentRoom.LocalPosition(player.Position).ToString();
            return true;
        }
    }
}