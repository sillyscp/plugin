using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Exiled.API.Features;
using PluginAPI.Events;

namespace SillySCP
{
    public class Plugin : Plugin<Config>
    {
        public static Plugin Instance;
        public List<PlayerStat> PlayerStats;
        private EventHandler handler;
        public static DiscordSocketClient Client;

        public override void OnEnabled()
        {
            Instance = this;
            handler = new EventHandler();
            EventManager.RegisterEvents(this, handler);
            Task.Run(StartClient);
            base.OnEnabled();
        }
        
        public override void OnDisabled()
        {
            EventManager.UnregisterEvents(this, handler);
            handler = null;
            Task.Run(StopClient);
            base.OnDisabled();
        }
        
        private static Task Log(LogMessage msg)
        {
            PluginAPI.Core.Log.Info(msg.ToString());
            return Task.CompletedTask;
        }

        private static async Task StartClient()
        {
            var config = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages
            };
            Client = new DiscordSocketClient(config);
            Client.Log += Log;
            await Client.LoginAsync(TokenType.Bot, Instance.Config.Token);
            await Client.StartAsync();
            Instance.SetCustomStatus("0/30 players active");
            var textChannel = Instance.GetChannel(1279544677334253610);
            var message = Instance.GetMessage(textChannel, 1280910252325339311);
            var messageEmbed = message.Embeds.FirstOrDefault();
            if (messageEmbed == null) return;
            var embedBuilder = new EmbedBuilder()
                .WithTitle("Silly SCP Member List")
                .WithColor(Color.Blue)
                .WithDescription();
            Instance.SetMessage(textChannel, 1280910252325339311, embedBuilder.Build());
        }
        
        private static async Task StopClient()
        {
            await Client.StopAsync();
            await Client.LogoutAsync();
        }

        public void SetStatus()
        {
            var playerList = Player.List;
            var textChannel = GetChannel(1279544677334253610);
            string players = "";
            foreach (var player in playerList)
            {
                players += "- " + player.Nickname + "\n";
            }
            var embedBuilder = new EmbedBuilder()
                .WithTitle("Silly SCP Member List")
                .WithColor(Color.Blue)
                .WithDescription(players);
            SetMessage(textChannel, 1280910252325339311, embedBuilder.Build());
            SetCustomStatus(Server.PlayerCount + "/30 players active");
        }
        
        private SocketTextChannel GetChannel(ulong id)
        {
            var channel = Client.GetChannel(id);
            if (channel.GetChannelType() != ChannelType.Text) return null;
            var textChannel = (SocketTextChannel) channel;
            return textChannel;
        }

        private IMessage GetMessage(SocketTextChannel channel, ulong id)
        {
            var message = channel.GetMessageAsync(id).GetAwaiter().GetResult();
            if (message.Author.Id != Client.CurrentUser.Id) return null;
            return message;
        }
        
        private void SetMessage(SocketTextChannel channel, ulong id, Embed embed)
        {
            try
            {
                var message = GetMessage(channel, id);
                if (message.Author.Id != Client.CurrentUser.Id) return;
                channel.ModifyMessageAsync(id, msg =>
                {
                    msg.Embed = embed;
                    msg.Content = "";
                }).GetAwaiter().GetResult();
            }
            catch (Exception error)
            {
                PluginAPI.Core.Log.Error(error.ToString());
            }
        }

        private void SetCustomStatus(string status)
        {
            try
            {
                Client.SetCustomStatusAsync(status).GetAwaiter().GetResult();
            }
            catch (Exception error)
            {
                PluginAPI.Core.Log.Error(error.ToString());
            }
        }
    }
}