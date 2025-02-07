using Exiled.API.Enums;
using Exiled.API.Features;
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
        public void Init()
        {
            Exiled.Events.Handlers.Player.ChangingSpectatedPlayer += OnChangingSpectatedPlayer;
            Exiled.Events.Handlers.Player.Died += OnPlayerDead;
            Exiled.Events.Handlers.Player.Spawned += OnSpawned;
            Exiled.Events.Handlers.Player.Left += OnPlayerLeave;
            Exiled.Events.Handlers.Server.RespawningTeam += OnRespawn;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            Exiled.Events.Handlers.Player.Hurt += OnHurt;
        }

        public void Unregister()
        {
            Exiled.Events.Handlers.Player.ChangingSpectatedPlayer -= OnChangingSpectatedPlayer;
            Exiled.Events.Handlers.Player.Died -= OnPlayerDead;
            Exiled.Events.Handlers.Player.Spawned -= OnSpawned;
            Exiled.Events.Handlers.Player.Left -= OnPlayerLeave;
            Exiled.Events.Handlers.Server.RespawningTeam -= OnRespawn;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Player.Hurt -= OnHurt;
        }

        private void OnHurt(HurtEventArgs ev)
        {
            if (ev.Attacker == null) return;
            if (ev.Attacker.IsScp) return;
            PlayerStat playerStat = ev.Attacker.FindOrCreatePlayerStat();
            if(playerStat.Damage == null) playerStat.Damage = 0;
            playerStat.Damage += ev.Amount;
        }

        private void OnChangingSpectatedPlayer(ChangingSpectatedPlayerEventArgs ev)
        {
            if (ev.NewTarget == null)
                return;
            PlayerStat playerStats = ev.Player.FindOrCreatePlayerStat();
            playerStats.Spectating = ev.NewTarget;
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
                ev.Player.ShowHint("", int.MaxValue);
                PlayerStat playerStats = ev.Player.FindPlayerStat();
                if (playerStats != null) playerStats.Spectating = null;
            }
        }

        private void OnPlayerLeave(LeftEventArgs ev)
        {
            PlayerStat playerStats = ev.Player.FindPlayerStat();
            if (playerStats != null) playerStats.Spectating = null;
        }

        private void OnRespawn(RespawningTeamEventArgs ev)
        {
            ev.Players.ForEach(p =>
            {
                p.ShowHint("", int.MaxValue);
                PlayerStat playerStats = p.FindPlayerStat();
                if (playerStats == null) return;
                playerStats.Spectating = null;
            });
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
            
            string message = "";
            
            if(normalMvpMessage != null) message += normalMvpMessage + "\n";
            if(scpMvpMessage != null) message += scpMvpMessage + "\n";
            if(damageMvpMessage != null) message += damageMvpMessage;

            message = message.Trim();

            Exiled.API.Features.Map.Broadcast(
                10,
                message
            );
        }

        private void OnRoundStarted()
        {
            Plugin.Instance.PlayerStats = new();
        }
    }
}