using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Console;
using LabApi.Features.Stores;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using RueI.API;
using RueI.API.Elements;
using SecretAPI.Attribute;
using SillySCP.API.Modules;

namespace SillySCP.API.Features
{
    public class PlayerStatDataStore
    {
        [CallOnLoad]
        public static void Load()
        {
            PlayerEvents.ChangedRole += OnChangedRole;
            PlayerEvents.Joined += OnJoined;
            PlayerEvents.Left += OnLeft;
        }

        [CallOnUnload]
        public static void Unload()
        {
            PlayerEvents.ChangedRole -= OnChangedRole;
            PlayerEvents.Joined -= OnJoined;
            PlayerEvents.Left -= OnLeft;
        }

        public static void OnChangedRole(PlayerChangedRoleEventArgs ev) =>
            RueDisplay.Get(ev.Player).Update();

        public static void OnJoined(PlayerJoinedEventArgs ev) =>
            Get(ev.Player).Setup();

        public static void OnLeft(PlayerLeftEventArgs ev) =>
            _dictionary.Remove(ev.Player);

        private static Dictionary<Player, PlayerStatDataStore> _dictionary = new();

        public static PlayerStatDataStore Get(Player player) =>
            _dictionary.GetOrAdd(player, () => new PlayerStatDataStore(player));
        
        private PlayerStatDataStore(Player player)
        {
            Owner = player;
            
            Element = new DynamicElement(300, ContentGetter)
            {
                ShowToSpectators = true
            };
            
            Kills = 0;
            ScpKills = 0;
            Damage = 0;
            PainkillersUsed = 0;
        }

        public Player Owner { get; }
        
        private void Setup() => RueDisplay.Get(Owner).Show(new Tag("Kill Count Display"), Element);

        public int Kills
        {
            get;
            set
            {
                if (Owner.DoNotTrack)
                    return;
                
                field = value;
                RueDisplay.Get(Owner).Update();
            }
        }

        public int ScpKills
        {
            get;
            set
            {
                if (Owner.DoNotTrack)
                    return;
                
                field = value;
                RueDisplay.Get(Owner).Update();
            }
        }

        public float Damage
        {
            get;
            set
            {
                if (Owner.DoNotTrack)
                    return;

                field = value;
            }
        }

        public int PainkillersUsed
        {
            get;
            set
            {
                if (Owner.DoNotTrack)
                    return;

                field = value;
            }
        }

        public DynamicElement Element { get; }

        private string ContentGetter(ReferenceHub hub)
        {
            if(Owner.ReferenceHub == hub || hub.roleManager.CurrentRole.Team != Team.Dead)
                return string.Empty;

            if (Owner.DoNotTrack)
                return $"Kill Count: Unknown";
            
            int kills = hub.roleManager.CurrentRole.Team == Team.SCPs ? ScpKills : Kills;

            return $"Kill Count: {kills}";
        }
    }
}
