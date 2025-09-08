using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Console;
using PlayerRoles;
using PlayerStatsSystem;
using SecretAPI.Features;
using SillySCP.API.Extensions;
using SillySCP.API.Features;

namespace SillySCP.Handlers
{
    public class PlayerStatHandler : IRegister
    {
        private LabApi.Features.Wrappers.Player _firstPlayerEscaped;
        private TimeSpan _escapeTime = TimeSpan.Zero;
        
        public void TryRegister()
        {
            PlayerEvents.Death += OnPlayerDead;
            PlayerEvents.Spawned += OnSpawned;
            ServerEvents.RoundEnded += OnRoundEnded;
            ServerEvents.RoundStarted += OnRoundStarted;
            PlayerEvents.Hurt += OnHurt;
            PlayerEvents.Escaping += OnEscape;

            PlayerEvents.UsedItem += OnUsedItem;
        }
        
        public void TryUnregister()
        {
            PlayerEvents.Death -= OnPlayerDead;
            PlayerEvents.Spawned -= OnSpawned;
            ServerEvents.RoundEnded -= OnRoundEnded;
            ServerEvents.RoundStarted -= OnRoundStarted;
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

            PlayerStat playerStat = ev.Player.FindOrCreatePlayerStat();
            playerStat.PainkillersUsed++;
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
            PlayerStat playerStat = ev.Attacker.FindOrCreatePlayerStat();
            if(playerStat.Damage == null) playerStat.Damage = 0;
            playerStat.Damage += handler.Damage;
        }

        private void OnPlayerDead(PlayerDeathEventArgs ev)
        {
            if (ev.DamageHandler is UniversalDamageHandler handler && handler.TranslationId == DeathTranslations.PocketDecay.Id)
            {
                LabApi.Features.Wrappers.Player scp106 = Plugin.Instance.Scp106 ??
                             LabApi.Features.Wrappers.Player.List.FirstOrDefault(p => p.Role == RoleTypeId.Scp106);
                scp106.UpdateKills();
            }

            if (ev.Attacker == null)
                return;
            if (ev.Player == ev.Attacker)
                return;
            ev.Attacker.UpdateKills();
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
            PlayerStat highestKiller = Plugin
                .Instance.PlayerStats.Where(p => p.Kills > 0).OrderByDescending(p => p.Kills)
                .FirstOrDefault();
            PlayerStat scpHighestKiller = Plugin
                .Instance.PlayerStats.Where(p => p.ScpKills > 0).OrderByDescending(p => p.ScpKills)
                .FirstOrDefault();
            PlayerStat highestDamage = Plugin.Instance.PlayerStats.OrderByDescending(p => p.Damage).FirstOrDefault();
            PlayerStat highestPainkillers =
                Plugin.Instance.PlayerStats.Where(p => p.PainkillersUsed > 0).OrderByDescending(p => p.PainkillersUsed).FirstOrDefault();
            
            LabApi.Features.Wrappers.Server.FriendlyFire = true;

            string normalMvpMessage = highestKiller != null
                ? $"Highest kills as a human was {highestKiller.Player.Nickname} with {highestKiller.Kills}"
                : null;
            string scpMvpMessage = scpHighestKiller != null
                ? $"Highest kills as an SCP was {scpHighestKiller.Player.Nickname} with {scpHighestKiller.ScpKills}"
                : null;
            string damageMvpMessage = highestDamage != null
                ? $"Highest damage was {highestDamage.Player.Nickname} with {Convert.ToInt32(highestDamage.Damage)}"
                : null;
            string firstEscapeMessage = _firstPlayerEscaped != null
                ? $"First to escape was {_firstPlayerEscaped.Nickname} in {_escapeTime.Minutes.ToString("D1")}m {_escapeTime.Seconds.ToString("D2")}s"
                : null;
            string painkillers = highestPainkillers != null
                ? $"Highest HRT usage: {highestPainkillers.Player.Nickname} with {highestPainkillers.PainkillersUsed}"
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

        private void OnRoundStarted()
        {
            Plugin.Instance.PlayerStats = new();
        }
    }
}