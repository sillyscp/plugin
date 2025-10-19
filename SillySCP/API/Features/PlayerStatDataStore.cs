using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Stores;
using LabApi.Features.Wrappers;
using PlayerRoles;
using RueI.API;
using RueI.API.Elements;
using SecretAPI.Attribute;
using SillySCP.API.Modules;

namespace SillySCP.API.Features
{
    public class PlayerStatDataStore : CustomDataStore
    {
        [CallOnLoad]
        public static void Load()
        {
            PlayerEvents.ChangedRole += OnChangedRole;
            PlayerEvents.Joined += OnJoined;
        }

        [CallOnUnload]
        public static void Unload()
        {
            PlayerEvents.ChangedRole -= OnChangedRole;
            PlayerEvents.Joined -= OnJoined;
        }

        public static void OnChangedRole(PlayerChangedRoleEventArgs ev)
        {
            RueDisplay.Get(ev.Player).Update();
        }

        public static void OnJoined(PlayerJoinedEventArgs ev)
        {
            ev.Player.GetDataStore<PlayerStatDataStore>();
        }
        
        public PlayerStatDataStore(Player player)
            : base(player)
        {
            Element = new DynamicElement(300, ContentGetter)
            {
                ShowToSpectators = true
            };
            RueDisplay.Get(Owner).Show(new Tag("Kill Count Display"), Element);
            
            Kills = 0;
            ScpKills = 0;
            Damage = 0;
            PainkillersUsed = 0;
        }

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
