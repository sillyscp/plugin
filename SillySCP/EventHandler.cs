using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using PlayerRoles;
using PlayerStatsSystem;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using Player = PluginAPI.Core.Player;

namespace SillySCP
{
    public class EventHandler
    {
        [PluginEvent(ServerEventType.PlayerDeath)]
        void OnPlayerDied(Player player, Player attacker, DamageHandlerBase handler)
        {
            if (attacker == null) return;
            if (player == attacker) return;
            Plugin.Instance.UpdateKills(attacker);
        }

        [PluginEvent(ServerEventType.RoundStart)]
        void OnRoundStart()
        {
            Server.FriendlyFire = false;
            Plugin.Instance.PlayerStats = new List<PlayerStat>();
            Plugin.Instance.RoundStarted = true;
            Plugin.Instance.SetStatus();
        }

        [PluginEvent(ServerEventType.RoundEnd)]
        void OnRoundEnd(RoundSummary.LeadingTeam _)
        {
            var playerStats = Plugin.Instance.PlayerStats.OrderByDescending((p) => p.Kills).ToList();
            var mvp = playerStats.FirstOrDefault();
            Server.FriendlyFire = true;
            if (mvp == null) return;
            Map.Broadcast(10, "MVP for this round is " + mvp.Player.Nickname + " with " + mvp.Kills + " kills!");
            Plugin.Instance.RoundStarted = false;
            Plugin.Instance.SetStatus();
        }
        
        [PluginEvent(ServerEventType.PlayerJoined)]
        void OnPlayerJoined(Player _)
        {
            if(Plugin.Instance.RoundStarted) Plugin.Instance.SetStatus();
        }

        [PluginEvent(ServerEventType.PlayerLeft)]
        void OnPlayerLeft(Player _)
        {
            Plugin.Instance.RoundStarted = Server.PlayerCount > 0;
            if(Plugin.Instance.RoundStarted) Plugin.Instance.SetStatus();
        }

        [PluginEvent(ServerEventType.PlayerChangeSpectator)]
        void OnPlayerChangeSpectator(Player player, Player _, Player spec)
        {
            if (spec == null) return;
            var playerKills = Plugin.Instance.PlayerStats.Find((p) => p.Player == spec)?.Kills;
            if (playerKills == null) playerKills = 0;
            player.ReceiveHint("Kill count: " + (spec.DoNotTrack ? "Unknown" : playerKills.ToString()) + "", int.MaxValue);
        }

        [PluginEvent(ServerEventType.PlayerSpawn)]
        public void OnSpawn(Player player, RoleTypeId id)
        {
            if(id == RoleTypeId.Scp106 && player.DoNotTrack == false) Plugin.Instance.Scp106 = player;
            player.ReceiveHint("", int.MaxValue);
        }

        [PluginEvent(ServerEventType.WaitingForPlayers)]
        public void WaitingForPlayers()
        {
            Plugin.Instance.RoundStarted = false;
            Plugin.Instance.SetStatus();
        }

        [PluginEvent]
        public void OnRespawn(TeamRespawnEvent ev)
        {
            ev.Players.ForEach((p) => p.ReceiveHint("", int.MaxValue));
        }

        [PluginEvent]
        public void OnScp049ResurrectBody(Scp049ResurrectBodyEvent ev)
        {
            ev.Target.ReceiveHint("", int.MaxValue);
        }

        [PluginEvent]
        public void OnPlayerEnterPocketDimension(PlayerEnterPocketDimensionEvent ev)
        {
            if(Plugin.Instance.Scp106 == null) return;
            Plugin.Instance.UpdateKills(Plugin.Instance.Scp106);
        }
        
        [PluginEvent]
        public void OnPlayerExitPocketDimension(PlayerExitPocketDimensionEvent ev)
        {
            if(Plugin.Instance.Scp106 == null) return;
            Plugin.Instance.UpdateKills(Plugin.Instance.Scp106, false);
        }
    }
}