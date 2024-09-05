using System;
using System.Collections.Generic;
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
        private bool statusUpdating;

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
            Client.Ready += Instance.Ready;
            await Client.LoginAsync(TokenType.Bot, Instance.Config.Token);
            await Client.StartAsync();
        }
        
        private static async Task StopClient()
        {
            await Client.StopAsync();
            await Client.LogoutAsync();
        }

        public void SetStatus()
        {
            string players = "";
            var textChannel = GetChannel(1279544677334253610);
            var playerList = Player.List;
            foreach (var player in playerList)
            {
                players += "- " + player.Nickname + "\n";
            }
            var embedBuilder = new EmbedBuilder()
                .WithTitle("Silly SCP Member List")
                .WithColor(Color.Blue)
                .WithDescription(players == "" ? "No players online" : players);
            SetMessage(textChannel, 1280910252325339311, embedBuilder.Build());
            SetCustomStatus();
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

        private void SetCustomStatus()
        {
            var status = Server.PlayerCount + "/30 players active";
            try
            {
                Client.SetCustomStatusAsync(status).GetAwaiter().GetResult();
                statusUpdating = false;
            }
            catch (Exception error)
            {
                PluginAPI.Core.Log.Error(error.ToString());
                if (statusUpdating == false)
                {
                    statusUpdating = true;
                    Task.Delay(5);
                    SetCustomStatus();
                }
            }
        }

        private async Task Ready()
        {
            SetStatus();
        }
    }
}