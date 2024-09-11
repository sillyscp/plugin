using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Exiled.API.Features;
using MEC;
using PluginAPI.Events;

namespace SillySCP
{
    public class Plugin : Plugin<Config>
    {
        public static Plugin Instance;
        public List<PlayerStat> PlayerStats;
        private EventHandler handler;
        public static DiscordSocketClient Client;
        public bool RoundStarted { get; set; }

        public override void OnEnabled()
        {
            RoundStarted = false;
            Instance = this;
            handler = new EventHandler();
            EventManager.RegisterEvents(this, handler);
            Task.Run(StartClient);
            base.OnEnabled();
        }
        
        public override void OnDisabled()
        {
            RoundStarted = false;
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
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages,
                LogLevel = LogSeverity.Error
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
                .WithDescription(RoundStarted ? players == "" ? "No players online" : players : "Waiting for players.");
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

        private async Task Ready()
        {
            SetStatus();
        }
        
        private void SetMessage(SocketTextChannel channel, ulong messageId, Embed embed)
        {
            var message = channel.GetMessageAsync(messageId).Result;

            try
            {
                channel.ModifyMessageAsync(message.Id, msg =>
                {
                    msg.Embed = embed;
                    msg.Content = "";
                });
            }
            catch
            {
                // ignored
            }
        }

        private void SetCustomStatus()
        {
            var status = RoundStarted
                ? $"{Server.PlayerCount}/{Server.MaxPlayerCount} players"
                : "Waiting for players.";
            try{
                Client.SetCustomStatusAsync(status);
            }
            catch
            {
                // ignored
            }
        }
    }
}