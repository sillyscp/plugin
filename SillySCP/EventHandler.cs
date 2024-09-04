using System.Collections.Generic;
using System.Linq;
using System.Windows.Interop;
using AdminToys;
using Discord;
using Discord.WebSocket;
using Exiled.API.Features;
using PlayerStatsSystem;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using Player = PluginAPI.Core.Player;

namespace SillySCP
{
    public class EventHandler
    {
        [PluginEvent(ServerEventType.PlayerDamagedShootingTarget)]
        void OnPlayerDamagedShootingTarget(Player player, ShootingTarget target, DamageHandlerBase damageHandler, float damage)
        {
            if (player == null) return;
            var playerStat = Plugin.Instance.PlayerStats.Find((p) => p.Player == player);
            if (playerStat == null)
            {
                playerStat = new PlayerStat
                {
                    Player = player,
                    Damage = 0
                };
                Plugin.Instance.PlayerStats.Add(playerStat);
            }

            playerStat.Damage += damage;
        }

        [PluginEvent(ServerEventType.RoundStart)]
        void OnRoundStart()
        {
            Plugin.Instance.PlayerStats = new List<PlayerStat>();
        }

        [PluginEvent(ServerEventType.RoundEnd)]
        void OnRoundEnd(RoundSummary.LeadingTeam leadingTeam)
        {
            var playerStats = Plugin.Instance.PlayerStats.OrderByDescending((p) => p.Damage).ToList();
            var mvp = playerStats.FirstOrDefault();
            if (mvp == null) return;
            Map.Broadcast(10, "MVP for this round is " + mvp.Player.Nickname + " with " + mvp.Damage + " damage!");
        }
        
        [PluginEvent(ServerEventType.PlayerJoined)]
        void OnPlayerJoined(Player player)
        {
            Plugin.Instance.SetStatus();
        }

        [PluginEvent(ServerEventType.PlayerLeft)]
        void OnPlayerLeft(Player player)
        {
            Plugin.Instance.SetStatus();
        }
    }
}