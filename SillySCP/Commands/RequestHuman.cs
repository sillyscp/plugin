using CommandSystem;
using Exiled.API.Features;
using SillySCP.API.Features;


namespace SillySCP.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class RequestHuman : ICommand
    {
        public string Command { get; } = "RequestHuman";

        public string[] Aliases { get; } = ["r","rh","request"];

        public string Description { get; } = "request to change into a human, if you're an SCP."; // no clue what the description for this should actually be tbh

        public bool Execute(
            ArraySegment<string> arguments,
            ICommandSender sender,
            out string response
        )
        {
            if (!Player.TryGet(sender, out Player player))
            {
                response = "Only Players can use this command.";
                return false; 
            }

            if (!player.IsScp)
            {
                response = "Only SCP's can use this command.";
                return false;
            }
            
            response = "Opening a volunteer request."; // I really do not know what terminology should be used, always sounds so weird
            VolunteerSystem.NewVolunteer(player.Role,original:player);
            return true;
        }
    }
}