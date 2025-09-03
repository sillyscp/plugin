using CommandSystem;
using LabApi.Features.Wrappers;

namespace SillySCP.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class LocalPosition : ICommand
    {
        public string Command { get; } = "LocalPosition";

        public string[] Aliases { get; } = new [] { "lp" };

        public string Description { get; } = "";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!Plugin.Instance.Config.Debug)
            {
                response = "Inaccessible";
                return false;
            }
            Player player = Player.Get(sender);
            response = $"{player?.Room?.Transform.InverseTransformPoint(player.Position).ToString()}, current room: {player.Room.GameObject.name}";
            return true;
        }
    }
}