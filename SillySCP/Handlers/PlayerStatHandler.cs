using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using PlayerRoles;
using PlayerStatsSystem;
using SecretAPI.Features;
using SillySCP.API.Features;
using LabPlayer = LabApi.Features.Wrappers.Player;

namespace SillySCP.Handlers
{

    public class PlayerStatHandler : IRegister
    {
        private LabPlayer _firstPlayerEscaped;
        private TimeSpan _escapeTime = TimeSpan.Zero;
        
        public void TryRegister()
        {
            PlayerEvents.Death += OnPlayerDead;
            PlayerEvents.Spawned += OnSpawned;
            ServerEvents.RoundEnded += OnRoundEnded;
            PlayerEvents.Hurt += OnHurt;
            PlayerEvents.Escaping += OnEscape;

            PlayerEvents.UsedItem += OnUsedItem;
        }
        
        public void TryUnregister()
        {
            PlayerEvents.Death -= OnPlayerDead;
            PlayerEvents.Spawned -= OnSpawned;
            ServerEvents.RoundEnded -= OnRoundEnded;
            PlayerEvents.Hurt -= OnHurt;
            PlayerEvents.Escaping -= OnEscape;

            PlayerEvents.UsedItem -= OnUsedItem;
        }

        private static void OnUsedItem(PlayerUsedItemEventArgs ev)
        {
            if (ev.UsableItem.Type != ItemType.Painkillers)
                return;

            if (ev.Player.DoNotTrack)
                return;

            ev.Player.GetDataStore<PlayerStatDataStore>().PainkillersUsed++;
        }

        private void OnEscape(PlayerEscapingEventArgs ev)
        {
            if (_firstPlayerEscaped == null && ev.IsAllowed)
            {
                _firstPlayerEscaped = ev.Player;
                _escapeTime = TimeSpan.FromSeconds(ev.Player.RoleBase.ActiveTime);
            }
        }

        private void OnHurt(PlayerHurtEventArgs ev)
        {
            if (ev.Attacker == null) return;
            if (ev.Attacker.IsSCP) return;
            if (ev.DamageHandler is not StandardDamageHandler handler) return;
            PlayerStatDataStore store = ev.Attacker.GetDataStore<PlayerStatDataStore>();
            store.Damage += handler.TotalDamageDealt;
        }

        private void OnPlayerDead(PlayerDeathEventArgs ev)
        {
            if (ev.DamageHandler is UniversalDamageHandler handler && handler.TranslationId == DeathTranslations.PocketDecay.Id)
            {
                LabPlayer scp106 = Plugin.Instance.Scp106 ??
                               LabPlayer.List.FirstOrDefault(p => p.Role == RoleTypeId.Scp106);
                if (scp106 == null)
                    return;
                
                PlayerStatDataStore scp106Store = scp106.GetDataStore<PlayerStatDataStore>();
                scp106Store.ScpKills++;
            }

            if (ev.Attacker == null)
                return;
            if (ev.Player == ev.Attacker)
                return;
            PlayerStatDataStore store = ev.Attacker.GetDataStore<PlayerStatDataStore>();
            if (ev.Attacker.IsSCP)
                store.ScpKills++;
            else
                store.Kills++;
        }

        private void OnSpawned(PlayerSpawnedEventArgs ev)
        {
            if (ev.Player.IsAlive)
            {
                if (ev.Player.Role == RoleTypeId.Scp106 && !ev.Player.DoNotTrack)
                    Plugin.Instance.Scp106 = ev.Player;
            }
        }

        private void OnRoundEnded(RoundEndedEventArgs _)
        {
            PlayerStatDataStore highestKiller = null;
            PlayerStatDataStore scpHighestKiller = null;
            PlayerStatDataStore highestDamage = null;
            PlayerStatDataStore highestPainkillers = null;
            
            foreach (LabPlayer player in LabPlayer.ReadyList)
            {
                PlayerStatDataStore store = player.GetDataStore<PlayerStatDataStore>();
                
                if(highestKiller == null || highestKiller.Kills < store.Kills)
                    highestKiller = store;
                
                if(scpHighestKiller == null || scpHighestKiller.Kills < store.ScpKills)
                    scpHighestKiller = store;
                
                if(highestDamage == null || highestDamage.Kills < store.Damage)
                    highestDamage = store;
                
                if(highestPainkillers == null  || highestPainkillers.Kills < store.PainkillersUsed)
                    highestPainkillers = store;
            }
            
            if(highestKiller?.Kills == 0)
                highestKiller = null;
            
            if(scpHighestKiller?.ScpKills == 0)
                scpHighestKiller = null;
            
            if(highestDamage?.Damage == 0)
                highestDamage = null;
            
            if(highestPainkillers?.PainkillersUsed == 0)
                highestPainkillers = null;
            
            LabApi.Features.Wrappers.Server.FriendlyFire = true;

            string normalMvpMessage = highestKiller != null
                ? $"Highest kills as a human was {highestKiller.Owner.Nickname} with {highestKiller.Kills}"
                : null;
            string scpMvpMessage = scpHighestKiller != null
                ? $"Highest kills as an SCP was {scpHighestKiller.Owner.Nickname} with {scpHighestKiller.ScpKills}"
                : null;
            string damageMvpMessage = highestDamage != null
                ? $"Highest damage was {highestDamage.Owner.Nickname} with {Convert.ToInt32(highestDamage.Damage)}"
                : null;
            string firstEscapeMessage = _firstPlayerEscaped != null
                ? $"First to escape was {_firstPlayerEscaped.Nickname} in {_escapeTime.Minutes.ToString("D1")}m {_escapeTime.Seconds.ToString("D2")}s"
                : null;
            string painkillers = highestPainkillers != null
                ? $"Highest HRT usage: {highestPainkillers.Owner.Nickname} with {highestPainkillers.PainkillersUsed}"
                : null;
            
            string message = string.Empty;
            
            if (normalMvpMessage != null) message += normalMvpMessage + "\n";
            if (scpMvpMessage != null) message += scpMvpMessage + "\n";
            if (damageMvpMessage != null) message += damageMvpMessage + "\n";
            if (firstEscapeMessage != null) message += firstEscapeMessage + "\n";
            if (painkillers != null) message += painkillers;

            message = "<size=0.6em>" + message.Trim() + "</size>";

            LabApi.Features.Wrappers.Server.SendBroadcast(
                message,
                10
            );

            _firstPlayerEscaped = null;
        }
    }
}