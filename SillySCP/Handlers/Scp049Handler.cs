using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp049Events;
using LabApi.Events.Handlers;
using LabApi.Features.Console;
using PlayerRoles;
using SecretAPI.Features;
using SillySCP.API.Features;

namespace SillySCP.Handlers
{
    public class Scp049Handler : IRegister
    {
        public void TryRegister()
        {
            Scp049Events.ResurrectedBody += OnRevived;
            PlayerEvents.Dying += OnDying;
            PlayerEvents.Joined += OnJoined;
            ServerEvents.RoundRestarted += OnRoundRestarted;
        }

        public void TryUnregister()
        {
            Scp049Events.ResurrectedBody -= OnRevived;
            PlayerEvents.Dying -= OnDying;
            PlayerEvents.Joined -= OnJoined;
            ServerEvents.RoundRestarted -= OnRoundRestarted;
        }
        
        public static void OnRevived(Scp049ResurrectedBodyEventArgs ev)
        {
            Scp049DataStore store = ev.Player.GetDataStore<Scp049DataStore>();
            store.ActivePlayers.Add(ev.Target);
        }

        public static void OnDying(PlayerDyingEventArgs ev)
        {
            if(ev.Player.Role == RoleTypeId.Scp049)
                Scp049DataStore.ActiveStores.Remove(Scp049DataStore.ActiveStores.FirstOrDefault(store => store.Owner == ev.Player));
            
            if (ev.Player.Role != RoleTypeId.Scp0492)
                return;

            foreach (Scp049DataStore store in Scp049DataStore.ActiveStores.Where(store => store.ActivePlayers.Contains(ev.Player)))
            {
                store.ActivePlayers.Remove(ev.Player);
            }
        }

        public static void OnJoined(PlayerJoinedEventArgs ev)
        {
            Scp049DataStore store = Scp049DataStore.ActiveStores.FirstOrDefault(store => store.Leavers.ContainsKey(ev.Player.UserId));
            if (store == null)
                return;
            
            store.ActivePlayers.Add(ev.Player);
            
            LastKnownInformation info = store.Leavers[ev.Player.UserId];
            
            ev.Player.Role = RoleTypeId.Scp0492;
            ev.Player.MaxHealth = info.MaxHealth;
            ev.Player.Health = info.Health;
            ev.Player.MaxHumeShield = info.MaxHumeShield;
            ev.Player.HumeShield = info.HumeShield;
            ev.Player.Position = info.Position;
            
            store.Leavers.Remove(ev.Player.UserId);
        }

        public static void OnRoundRestarted()
        {
            Scp049DataStore.ActiveStores.Clear();
        }
    }
}