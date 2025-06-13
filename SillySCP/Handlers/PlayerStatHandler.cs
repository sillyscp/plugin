using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
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
            LabApi.Events.Handlers.PlayerEvents.Death += OnPlayerDead;
            LabApi.Events.Handlers.PlayerEvents.Spawned += OnSpawned;
            LabApi.Events.Handlers.ServerEvents.RoundEnded += OnRoundEnded;
            LabApi.Events.Handlers.ServerEvents.RoundStarted += OnRoundStarted;
            LabApi.Events.Handlers.PlayerEvents.Hurt += OnHurt;
            LabApi.Events.Handlers.PlayerEvents.Escaping += OnEscape;
        }
        
        public void TryUnregister()
        {
            LabApi.Events.Handlers.PlayerEvents.Death -= OnPlayerDead;
            LabApi.Events.Handlers.PlayerEvents.Spawned -= OnSpawned;
            LabApi.Events.Handlers.ServerEvents.RoundEnded -= OnRoundEnded;
            LabApi.Events.Handlers.ServerEvents.RoundStarted -= OnRoundStarted;
            LabApi.Events.Handlers.PlayerEvents.Hurt -= OnHurt;
            LabApi.Events.Handlers.PlayerEvents.Escaping -= OnEscape;
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
            
            string message = "";
            
            if(normalMvpMessage != null) message += normalMvpMessage + "\n";
            if(scpMvpMessage != null) message += scpMvpMessage + "\n";
            if(damageMvpMessage != null) message += damageMvpMessage + "\n";
            if(firstEscapeMessage != null) message += firstEscapeMessage;

            message = message.Trim();

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