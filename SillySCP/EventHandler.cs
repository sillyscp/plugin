using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using Respawning;
using UnityEngine;
using Player = PluginAPI.Core.Player;
using Round = PluginAPI.Core.Round;

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
            Plugin.Instance.UpdateKills(attacker, attacker.IsSCP);
        }

        [PluginEvent(ServerEventType.RoundStart)]
        public void OnRoundStart()
        {
            Server.FriendlyFire = false;
            Plugin.Instance.PlayerStats = new List<PlayerStat>();
            Plugin.Instance.Volunteers = new List<Volunteers>();
            Timing.RunCoroutine(Plugin.Instance.DisableVolunteers());
            Plugin.Instance.SetStatus(true);
            var eventRound = Plugin.Instance.RoundEvents.EventRound();
            if (eventRound)
            {
                var eventChosen = Plugin.Instance.RoundEvents.PlayRandomEvent();
                Plugin.Instance.ChosenEvent = eventChosen.Item1;
                Map.Broadcast(10, eventChosen.Item2);
            }
            else
            {
                Plugin.Instance.ChosenEvent = null;
            }

            var message = "Round has started with the following people:\n```";
            message += string.Join("\n", Player.GetPlayers().Select(player => player.Nickname));
            message += "```";
            Plugin.Client.GetGuild(1279504339248877588).GetTextChannel(1294978305253970002).SendMessageAsync(message);
        }

        [PluginEvent(ServerEventType.RoundEnd)]
        public void OnRoundEnd(RoundSummary.LeadingTeam _)
        {
            var highestKills = Plugin
                .Instance.PlayerStats.Where(p => !p.Player.IsSCP && p.Kills > 0).OrderByDescending((p) => p.Kills)
                .ToList();
            var scpHighestKills = Plugin
                .Instance.PlayerStats.Where(p => p.Player.IsSCP && p.ScpKills > 0).OrderByDescending((p) => p.ScpKills)
                .ToList();
            var highestKiller = highestKills.FirstOrDefault();
            var scpHighestKiller = scpHighestKills.FirstOrDefault();
            Server.FriendlyFire = true;
            
            var normalMvpMessage = highestKiller != null ? $"Highest kills from a human for this round is {highestKiller.Player.Nickname} with {highestKiller.Kills}" : null;
            var scpMvpMessage = scpHighestKiller != null ? $"Highest kills from an SCP for this round is {scpHighestKiller.Player.Nickname} with {scpHighestKiller.ScpKills}" : null;
            var message = $"{normalMvpMessage}\n{scpMvpMessage}".Trim();
            
            Map.Broadcast(
                10,
                message
            );
        }

        [PluginEvent(ServerEventType.RoundRestart)]
        public void OnRestart()
        {
            Server.FriendlyFire = false;
            Plugin.Instance.SetStatus();
        }
        
        [PluginEvent]
        public void OnPlayerJoined(PlayerJoinedEvent ev)
        {
            if (!Round.IsRoundEnded && Round.IsRoundStarted && ev.Player.Role == RoleTypeId.Spectator)
            {
                Timing.RunCoroutine(Plugin.Instance.RespawnTimer(ev.Player));
            }
            if(!String.IsNullOrEmpty(ev.Player.Nickname) && !Round.IsRoundEnded && Round.IsRoundStarted)
            {
                Plugin.Client.GetGuild(1279504339248877588).GetTextChannel(1294978305253970002)
                    .SendMessageAsync($"Player `{ev.Player.Nickname}` has joined the server");
                Plugin.Instance.SetStatus();
            }
        }

        [PluginEvent]
        public void OnChangeRole(PlayerChangeRoleEvent ev)
        {
            if (ev.NewRole == RoleTypeId.Spectator) Timing.RunCoroutine(Plugin.Instance.RespawnTimer(ev.Player));
            if (ev.OldRole.Team == Team.SCPs && (ev.NewRole == RoleTypeId.Spectator || ev.NewRole == RoleTypeId.None) && Plugin.Instance.ReadyVolunteers)
            {
                Cassie.Clear();
                var volunteer = new Volunteers
                {
                    Replacement = ev.Player.Role,
                    Players = new List<Exiled.API.Features.Player>()
                };
                Plugin.Instance.Volunteers.Add(volunteer);
                var scpNumber = Plugin.Instance.GetScpNumber(ev.Player.Role);
                if(scpNumber == null) return;
                Map.Broadcast(10, $"SCP-{scpNumber} has left the game\nPlease run .volunteer {scpNumber} to volunteer to be the SCP");
                Timing.RunCoroutine(Plugin.Instance.ChooseVolunteers(volunteer));
            }
        }

        [PluginEvent]
        public void OnPlayerLeft(PlayerLeftEvent ev)
        {
            var playerStats = Plugin.Instance.FindPlayerStat(ev.Player);
            if(playerStats != null) playerStats.Spectating = null;
            if(!Round.IsRoundEnded && Round.IsRoundStarted) Plugin.Instance.SetStatus();
        }

        [PluginEvent(ServerEventType.PlayerChangeSpectator)]
        public void OnPlayerChangeSpectator(Player spectator, Player _, Player observee)
        {
            if (observee == null)
                return;
            var playerStats = Plugin.Instance.FindOrCreatePlayerStat(spectator);
            playerStats.Spectating = observee;
        }

        [PluginEvent]
        public void OnSpawn(PlayerSpawnEvent ev)
        {
            if (ev.Role == RoleTypeId.None) return;
            if (ev.Role == RoleTypeId.Spectator)
            {
                Timing.RunCoroutine(Plugin.Instance.RespawnTimer(ev.Player));
            }

            if (ev.Role != RoleTypeId.Spectator || ev.Role != RoleTypeId.None)
            {
                if (ev.Role == RoleTypeId.Scp106 && ev.Player.DoNotTrack == false)
                    Plugin.Instance.Scp106 = ev.Player;
                ev.Player.ReceiveHint("", int.MaxValue);
                var playerStats = Plugin.Instance.FindPlayerStat(ev.Player);
                if (playerStats != null) playerStats.Spectating = null;
            }
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
            Plugin.Instance.UpdateKills(Plugin.Instance.Scp106, true);
        }

        [PluginEvent]
        public void OnPlayerExitPocketDimension(PlayerExitPocketDimensionEvent ev)
        {
            if (ev.Player.Role == RoleTypeId.Spectator) return;
            if (Plugin.Instance.Scp106 == null)
                return;
            Plugin.Instance.UpdateKills(Plugin.Instance.Scp106, true, false);
        }
    }
}
