using CommandSystem;
using Exiled.API.Extensions;
using Exiled.API.Features;
using PlayerRoles;

namespace SillySCP.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class TowerSit : ICommand
    {
        
        private void AddPlayer(Player player)
        {
            player.SessionVariables.Add("pre_sit_position",player.Position);
            player.IsGodModeEnabled = true;
            if (player.IsScp)
            {
                player.Broadcast(10,"If you go to Settings, Server-specific, you can set a bind for proximity chat");
            }
            player.Teleport(RoleTypeId.Tutorial.GetRandomSpawnLocation());
        }
        private static bool RestorePlayer(Player player)
        {
            if (!player.SessionVariables.TryGetValue("pre_sit_position", out var position)) return false; // would use Vector3 but it won't let me 
            player.Teleport(position);
            player.SessionVariables.Remove("pre_sit_position");
            player.IsGodModeEnabled = false;
            return true;
        }
        
        
        public string Command { get; } = "towersit";
        public string Description { get; } = "Teleport a player to the tower and return them after";
        public string[] Aliases { get; } = ["ts"];

        private const string Usage = "Invalid Usage, Applicable usages:" +
                                     "\n ts add <PLAYER> - teleport a player to the tower (Both IDs and Partial Usernames are accepted)" +
                                     "\n ts restore <PLAYER> - restore a players original position (Both IDs and Partial Usernames are accepted)";
        
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
            
            if (!Player.TryGet(arguments.At(1), out Player player))
            {
                response = "Must be a player!";
                return false;
            }

            if (player.IsDead)
            {
                response = "Player is dead!";
                return false;
            }
            
            switch (arguments.At(0))
            {
                case "add":
                    if (player.SessionVariables.ContainsKey("pre_sit_position"))
                    {
                        response = "Player already has a return position!";
                        return false;
                    }
                    AddPlayer(player);
                    response = $"{player.Nickname} teleporting to the tower!";
                    return true;
                
                case "restore":
                    if (!RestorePlayer(player))
                    {
                        response = $"Player {player.Nickname} doesnt have a return position!";
                        return false;
                    }
                    response = $"{player.Nickname} restored from the tower!";
                    return true;
                
                default:
                    response = Usage;
                    return false;
                
            }
            
        }
    }
}