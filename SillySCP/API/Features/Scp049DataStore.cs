using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp049Events;
using LabApi.Events.Handlers;
using LabApi.Features.Stores;
using LabApi.Features.Wrappers;
using PlayerRoles;
using SecretAPI.Features;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace SillySCP.API.Features
{
    public class LastKnownInformation
    {
        public Vector3 Position { get; set; }
        
        public float Health { get; set; }
        
        public float MaxHealth { get; set; }
        
        public float HumeShield { get; set; }
        
        public float MaxHumeShield { get; set; }
    }
    public class Scp049DataStore : CustomDataStore
    {
        public Scp049DataStore(Player player)
            : base(player)
        {
            ActiveStores.Add(this);
        }

        /// <summary>
        /// List of IDs of people who have left which should be SCP-049-2.
        /// </summary>
        public Dictionary<string, LastKnownInformation> Leavers { get; } = [];
        
        public List<Player> ActivePlayers { get; } = [];

        public static List<Scp049DataStore> ActiveStores { get; } = [];
    }
}