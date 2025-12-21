using CommandSystem;
using LabApi.Features.Extensions;
using LabApi.Features.Stores;
using LabApi.Features.Wrappers;
using PlayerRoles;
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
        private void AddPlayer(Player player)
        {
            player.GetDataStore<DataStore>().Position = player.Position;
            player.IsGodModeEnabled = true;
            if (player.IsSCP)
            {
                player.SendBroadcast("If you go to Settings, Server-specific, you can set a bind for proximity chat",10);
            }
            RoleTypeId.Tutorial.TryGetRandomSpawnPoint(out Vector3 position, out float _);
            player.Position = position;
        }
        private static bool RestorePlayer(Player player)
        {
            DataStore store = player.GetDataStore<DataStore>();
            player.Position = (Vector3)store.Position!;
            store.Position = null;
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

            if (!player.IsAlive)
            {
                response = "Player is dead!";
                return false;
            }
            
            switch (arguments.At(0))
            {
                case "add":
                    if (player.GetDataStore<DataStore>().Position != null)
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