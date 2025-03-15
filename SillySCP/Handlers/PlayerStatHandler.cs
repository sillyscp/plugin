using Exiled.API.Enums;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using PlayerRoles;
using SillySCP.API.Extensions;
using SillySCP.API.Features;
using SillySCP.API.Interfaces;

namespace SillySCP.Handlers
{
    public class PlayerStatHandler : IRegisterable
    {
        private Exiled.API.Features.Player _firstPlayerEscaped;
        private TimeSpan _escapeTime = TimeSpan.Zero;
        
        public void Init()
        {
            Exiled.Events.Handlers.Player.Died += OnPlayerDead;
            Exiled.Events.Handlers.Player.Spawned += OnSpawned;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            Exiled.Events.Handlers.Player.Hurt += OnHurt;
            Exiled.Events.Handlers.Player.Escaped += OnEscape;
        }

        public void Unregister()
        {
            Exiled.Events.Handlers.Player.Died -= OnPlayerDead;
            Exiled.Events.Handlers.Player.Spawned -= OnSpawned;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Player.Hurt -= OnHurt;
            Exiled.Events.Handlers.Player.Escaped -= OnEscape;
        }

        private void OnEscape(EscapedEventArgs ev)
        {
            if (_firstPlayerEscaped == null)
            {
                _firstPlayerEscaped = ev.Player;
                _escapeTime = TimeSpan.FromSeconds(ev.EscapeTime);
            }
        }

        private void OnHurt(HurtEventArgs ev)
        {
            if (ev.Attacker == null) return;
            if (ev.Attacker.IsScp) return;
            PlayerStat playerStat = ev.Attacker.FindOrCreatePlayerStat();
            if(playerStat.Damage == null) playerStat.Damage = 0;
            playerStat.Damage += ev.Amount;
        }

        private void OnPlayerDead(DiedEventArgs ev)
        {
            if (ev.DamageHandler.Type == DamageType.PocketDimension)
            {
                Exiled.API.Features.Player scp106 = Plugin.Instance.Scp106 ??
                             Exiled.API.Features.Player.List.FirstOrDefault(p => p.Role == RoleTypeId.Scp106);
                scp106.UpdateKills();
            }

            if (ev.Attacker == null)
                return;
            if (ev.Player == ev.Attacker)
                return;
            ev.Attacker.UpdateKills();
        }

        private void OnSpawned(SpawnedEventArgs ev)
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
            
            Exiled.API.Features.Server.FriendlyFire = true;

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

            Exiled.API.Features.Map.Broadcast(
                10,
                message
            );

            _firstPlayerEscaped = null;
        }

        private void OnRoundStarted()
        {
            Plugin.Instance.PlayerStats = new();
        }
    }
}