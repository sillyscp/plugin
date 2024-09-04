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
            Map.Broadcast(10, "MVP for this round is " + playerStats[0].Player.Nickname + " with " + playerStats[0].Damage + " damage!");
        }
        
        [PluginEvent(ServerEventType.PlayerJoined)]
        void OnPlayerJoined(Player player)
        {
            var textChannel = GetChannel();
            var message = GetMessage(textChannel);
            var messageEmbed = message.Embeds.FirstOrDefault();
            if (messageEmbed == null)
            {
                var embedBuilder = new EmbedBuilder()
                    .WithTitle("Silly SCP Member List")
                    .WithColor(Color.Blue)
                    .WithDescription("- " + player.Nickname);
                textChannel.ModifyMessageAsync(message.Id, msg => msg.Embed = embedBuilder.Build()).GetAwaiter().GetResult();
            }
            else
            {
                var embedBuilder = new EmbedBuilder()
                    .WithTitle("Silly SCP Member List")
                    .WithColor(Color.Blue)
                    .WithDescription(messageEmbed.Description + "- " + player.Nickname + "\n");
                textChannel.ModifyMessageAsync(message.Id, msg =>
                {
                    msg.Embed = embedBuilder.Build();
                    msg.Content = "";
                }).GetAwaiter().GetResult();
            }
            Plugin.Client.SetCustomStatusAsync(Server.PlayerCount + "/30 players active").GetAwaiter().GetResult();
        }

        [PluginEvent(ServerEventType.PlayerLeft)]
        void OnPlayerLeft(Player player)
        {
            var textChannel = GetChannel();
            var message = GetMessage(textChannel);
            var messageEmbed = message.Embeds.FirstOrDefault();
            if (messageEmbed == null) return;
            var embedBuilder = new EmbedBuilder()
                .WithTitle("Silly SCP Member List")
                .WithColor(Color.Blue)
                .WithDescription(messageEmbed.Description.Replace("- " + player.Nickname, "").Replace("\n\n", "\n"));
            textChannel.ModifyMessageAsync(message.Id, msg => msg.Embed = embedBuilder.Build()).GetAwaiter().GetResult();
            Plugin.Client.SetCustomStatusAsync(Server.PlayerCount + "/30 players active").GetAwaiter().GetResult();
        }

        private SocketTextChannel GetChannel()
        {
            var channel = Plugin.Client.GetChannel(1279544677334253610);
            if (channel.GetChannelType() != ChannelType.Text) return null;
            var textChannel = (SocketTextChannel) channel;
            return textChannel;
        }

        private IMessage GetMessage(SocketTextChannel channel)
        {
            var message = channel.GetMessageAsync(1280910252325339311).GetAwaiter().GetResult();
            if (message.Author.Id != Plugin.Client.CurrentUser.Id) return null;
            return message;
        }
    }
}