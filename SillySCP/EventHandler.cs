using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using PlayerStatsSystem;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using Player = PluginAPI.Core.Player;

namespace SillySCP
{
    public class EventHandler
    {
        [PluginEvent(ServerEventType.PlayerDeath)]
        void OnPlayerDied(Player _, Player attacker, DamageHandlerBase __)
        {
            if (attacker == null) return;
            var playerStat = Plugin.Instance.PlayerStats.Find((p) => p.Player == attacker);
            if (playerStat == null)
            {
                playerStat = new PlayerStat
                {
                    Player = attacker,
                    Kills = 0
                };
                Plugin.Instance.PlayerStats.Add(playerStat);
            }
            
            playerStat.Kills++;
        }

        [PluginEvent(ServerEventType.RoundStart)]
        void OnRoundStart()
        {
            Server.FriendlyFire = false;
            Plugin.Instance.PlayerStats = new List<PlayerStat>();
        }

        [PluginEvent(ServerEventType.RoundEnd)]
        void OnRoundEnd(RoundSummary.LeadingTeam _)
        {
            var playerStats = Plugin.Instance.PlayerStats.OrderByDescending((p) => p.Kills).ToList();
            var mvp = playerStats.FirstOrDefault();
            Server.FriendlyFire = true;
            if (mvp == null) return;
            Map.Broadcast(10, "MVP for this round is " + mvp.Player.Nickname + " with " + mvp.Kills + " kills!");
        }
        
        [PluginEvent(ServerEventType.PlayerJoined)]
        void OnPlayerJoined(Player _)
        {
            Plugin.Instance.SetStatus();
        }

        [PluginEvent(ServerEventType.PlayerLeft)]
        void OnPlayerLeft(Player _)
        {
            Plugin.Instance.SetStatus();
        }
    }
}