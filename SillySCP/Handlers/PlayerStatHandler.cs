using Exiled.API.Enums;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using MEC;
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
                var scp106 = Plugin.Instance.Scp106 ??
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
                var playerStats = ev.Player.FindPlayerStat();
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
            var highestKills = Plugin
                .Instance.PlayerStats.Where(p => p.Kills > 0).OrderByDescending(p => p.Kills)
                .ToList();
            var scpHighestKills = Plugin
                .Instance.PlayerStats.Where(p => p.ScpKills > 0).OrderByDescending(p => p.ScpKills)
                .ToList();
            var highestKiller = highestKills.FirstOrDefault();
            var scpHighestKiller = scpHighestKills.FirstOrDefault();
            Exiled.API.Features.Server.FriendlyFire = true;

            var normalMvpMessage = highestKiller != null
                ? $"Highest kills from a human for this round is {highestKiller.Player.Nickname} with {highestKiller.Kills}"
                : null;
            var scpMvpMessage = scpHighestKiller != null
                ? $"Highest kills from an SCP for this round is {scpHighestKiller.Player.Nickname} with {scpHighestKiller.ScpKills}"
                : null;
            var message = $"{normalMvpMessage}\n{scpMvpMessage}".Trim();

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