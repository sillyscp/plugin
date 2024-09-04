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
            var textChannel = Plugin.Instance.GetChannel(1279544677334253610);
            var message = Plugin.Instance.GetMessage(textChannel, 1280910252325339311);
            var messageEmbed = message.Embeds.FirstOrDefault();
            var embedBuilder = new EmbedBuilder()
                .WithTitle("Silly SCP Member List")
                .WithColor(Color.Blue);
            if (messageEmbed == null)
            {
                embedBuilder.WithDescription("- " + player.Nickname);
            }
            else
            {
                embedBuilder.WithDescription(Server.PlayerCount > 1 ? messageEmbed.Description + "\n- " + player.Nickname : messageEmbed.Description + "- " + player.Nickname);
            }
            Plugin.Instance.SetMessage(textChannel, 1280910252325339311, embedBuilder.Build());
            Plugin.Instance.SetCustomStatus(Server.PlayerCount + "/30 players active");
        }

        [PluginEvent(ServerEventType.PlayerLeft)]
        void OnPlayerLeft(Player player)
        {
            var textChannel = Plugin.Instance.GetChannel(1279544677334253610);
            var message = Plugin.Instance.GetMessage(textChannel, 1280910252325339311);
            var messageEmbed = message.Embeds.FirstOrDefault();
            if (messageEmbed == null) return;
            var embedBuilder = new EmbedBuilder()
                .WithTitle("Silly SCP Member List")
                .WithColor(Color.Blue)
                .WithDescription(messageEmbed.Description.Replace("- " + player.Nickname, "").Replace("\n\n", "\n"));
            Plugin.Instance.SetMessage(textChannel, 1280910252325339311, embedBuilder.Build());
            Plugin.Instance.SetCustomStatus(Server.PlayerCount + "/30 players active");
        }
    }
}