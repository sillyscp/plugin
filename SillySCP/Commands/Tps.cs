using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CommandSystem;
using LabApi.Features.Wrappers;

namespace SillySCP.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Tps : ICommand
    {
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = Server.Tps.ToString(CultureInfo.InvariantCulture);
            return true;
        }

        public string Command { get; } = "tps";
        public string[] Aliases { get; } = [];
        public string Description { get; } = "Get the current server TPS.";
    }
}