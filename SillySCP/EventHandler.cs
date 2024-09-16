using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using Respawning;
using Player = PluginAPI.Core.Player;

namespace SillySCP
{
    public class EventHandler
    {
        [PluginEvent(ServerEventType.PlayerDeath)]
        public void OnPlayerDied(Player player, Player attacker, DamageHandlerBase _)
        {
            Timing.RunCoroutine(Plugin.Instance.RespawnTimer(player));
            if (attacker == null)
                return;
            if (player == attacker)
                return;
            Plugin.Instance.UpdateKills(attacker);
        }

        [PluginEvent(ServerEventType.RoundStart)]
        public void OnRoundStart()
        {
            Server.FriendlyFire = false;
            Plugin.Instance.PlayerStats = new List<PlayerStat>();
            Plugin.Instance.Volunteers = new List<Volunteers>();
            Timing.RunCoroutine(Plugin.Instance.DisableVolunteers());
            Plugin.Instance.RoundStarted = true;
            Plugin.Instance.SetStatus();
        }

        [PluginEvent(ServerEventType.RoundEnd)]
        public void OnRoundEnd(RoundSummary.LeadingTeam _)
        {
            var playerStats = Plugin
                .Instance.PlayerStats.OrderByDescending((p) => p.Kills)
                .ToList();
            var mvp = playerStats.FirstOrDefault();
            Server.FriendlyFire = true;
            if (mvp == null)
                return;
            Map.Broadcast(
                10,
                "MVP for this round is " + mvp.Player.Nickname + " with " + mvp.Kills + " kills!"
            );
        }

        [PluginEvent(ServerEventType.RoundRestart)]
        public void OnRestart()
        {
            Plugin.Instance.RoundStarted = false;
            Plugin.Instance.SetStatus();
        }

        [PluginEvent(ServerEventType.PlayerJoined)]
        public void OnPlayerJoined(Player _)
        {
            if (Plugin.Instance.RoundStarted)
                Plugin.Instance.SetStatus();
        }

        [PluginEvent]
        public void OnChangeRole(PlayerChangeRoleEvent ev)
        {
            if (ev.OldRole.Team == Team.SCPs && Plugin.Instance.ReadyVolunteers)
            {
                Cassie.Clear();
                var volunteer = new Volunteers
                {
                    Replacement = ev.Player.Role,
                };
                Plugin.Instance.Volunteers.Add(volunteer);
                string scpNumber;
                switch (ev.Player.Role)
                {
                    case RoleTypeId.Scp049:
                        scpNumber = "049";
                        break;
                    case RoleTypeId.Scp079:
                        scpNumber = "079";
                        break;
                    case RoleTypeId.Scp096:
                        scpNumber = "096";
                        break;
                    case RoleTypeId.Scp106:
                        scpNumber = "106";
                        break;
                    case RoleTypeId.Scp939:
                        scpNumber = "939";
                        break;
                    default:
                        scpNumber = "unknown";
                        break;
                }
                Map.Broadcast(10, $"SCP-{scpNumber} has left the game\nPlease run .volunteer {scpNumber} to volunteer to be the SCP");
                Timing.RunCoroutine(Plugin.Instance.ChooseVolunteers(volunteer));
            }
        }

        [PluginEvent(ServerEventType.PlayerLeft)]
        public void OnPlayerLeft(Player player)
        {
            var playerStats = Plugin.Instance.FindPlayerStat(player);
            if(playerStats != null) playerStats.Spectating = null;
            Plugin.Instance.RoundStarted = Server.PlayerCount > 0;
            if (Plugin.Instance.RoundStarted)
                Plugin.Instance.SetStatus();
        }

        [PluginEvent(ServerEventType.PlayerChangeSpectator)]
        public void OnPlayerChangeSpectator(Player spectator, Player _, Player observee)
        {
            if (observee == null)
                return;
            var playerStats = Plugin.Instance.FindOrCreatePlayerStat(spectator);
            playerStats.Spectating = observee;
        }

        [PluginEvent(ServerEventType.PlayerSpawn)]
        public void OnSpawn(Player player, RoleTypeId id)
        {
            if (id == RoleTypeId.Spectator)
            {
                Timing.RunCoroutine(Plugin.Instance.RespawnTimer(player));
            }
            if (id == RoleTypeId.Spectator || id == RoleTypeId.None)
                return;
            if (id == RoleTypeId.Scp106 && player.DoNotTrack == false)
                Plugin.Instance.Scp106 = player;
            player.ReceiveHint("", int.MaxValue);
            var playerStats = Plugin.Instance.FindPlayerStat(player);
            if (playerStats == null) return;
            playerStats.Spectating = null;
        }

        [PluginEvent]
        public void OnRespawn(TeamRespawnEvent ev)
        {
            if (ev.Team == SpawnableTeamType.None)
                return;
            ev.Players.ForEach((p) =>
            {
                p.ReceiveHint("", int.MaxValue);
                var playerStats = Plugin.Instance.FindPlayerStat(p);
                if (playerStats == null) return;
                playerStats.Spectating = null;
            });
        }

        [PluginEvent]
        public void OnPlayerEnterPocketDimension(PlayerEnterPocketDimensionEvent ev)
        {
            if (Plugin.Instance.Scp106 == null)
                return;
            Plugin.Instance.UpdateKills(Plugin.Instance.Scp106);
        }

        [PluginEvent]
        public void OnPlayerExitPocketDimension(PlayerExitPocketDimensionEvent ev)
        {
            if (Plugin.Instance.Scp106 == null)
                return;
            Plugin.Instance.UpdateKills(Plugin.Instance.Scp106, false);
        }
    }
}
