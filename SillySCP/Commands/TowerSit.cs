using CommandSystem;
using LabApi.Features.Stores;
using LabApi.Features.Wrappers;
using SillySCP.API.Features;
using UnityEngine;


namespace SillySCP.Commands
{
    public class DataStore : CustomDataStore
    {
        public DataStore(Player player)
            : base(player)
        {}
        
        public Vector3? Position { get; set; }
    }
    
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class TowerSit : ICommand
    {
        public string Command { get; } = "towersit";
        public string Description { get; } = "Teleport a player to the tower and return them after";
        public string[] Aliases { get; } = ["ts"];

        private const string Usage = "Invalid Usage, Applicable usages:" +
                                     "\n ts add <PLAYER> - Create a towersit (Usernames are accepted)" +
                                     "\n ts restore <PLAYER> - End a towersit (Usernames are accepted)";
        
        public bool Execute(
            ArraySegment<string> arguments,
            ICommandSender sender,
            out string response
        )
        {
            if (arguments.Count != 2)
            {
                response = Usage;
                return false;
            }
            
            if (!Player.TryGetPlayersByName(arguments.At(1), out List<Player> players)) // no method for the first player to match
            {
                response = "Must be a player!";
                return false;
            }
            Player player = players.First();
            
            switch (arguments.At(0))
            {
                case "add":
                    if (TowerSitSystem.ActiveSits.ContainsKey(player))
                    {
                        response =$"{player.Nickname} is already in the tower!";
                        return false;
                    }
                    TowerSitSystem.Start(player);
                    response = $"{player.Nickname} has been towered!";
                    return true;
                
                case "restore":
                    if (!TowerSitSystem.ActiveSits.ContainsKey(player))
                    {
                        response = $"Player {player.Nickname} is not actively in the tower!";
                        return false;
                    }

                    TowerSitSystem.End(player);
                    response = $"returned {player.Nickname} from the tower!";
                    return true;
                
                default:
                    response = Usage;
                    return false;
                
            }
        }
    }
}