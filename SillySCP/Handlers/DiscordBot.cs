using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Exiled.API.Features;
using MEC;
using API = Exiled.API;

namespace SillySCP.Handlers
{
    public class DiscordBot
    {
        private DiscordSocketClient Client { get; set; }

        private SocketTextChannel _statusChannel;
        public SocketTextChannel DeathChannel;
        public SocketTextChannel ConnectionChannel;

        public SocketGuild Guild;
        
        private Task DiscLog(LogMessage msg)
        {
            PluginAPI.Core.Log.Info(msg.ToString());
            return Task.CompletedTask;
        }

        public async Task StartClient()
        {
            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages,
                LogLevel = LogSeverity.Error
            };
            Client = new DiscordSocketClient(config);
            Client.Log += DiscLog;
            Client.Ready += Ready;
            await Client.LoginAsync(TokenType.Bot, Plugin.Instance.Config.Token);
            await Client.StartAsync();
        }

        public async Task StopClient()
        {
            await Client.LogoutAsync();
            await Client.StopAsync();
        }

        private async Task Ready()
        {
            _statusChannel = GetChannel(1279544677334253610);
            DeathChannel = GetChannel(1296011257006002207);
            ConnectionChannel = GetChannel(1294978305253970002);
            Guild = Client.GetGuild(1279504339248877588);
            SetStatus();
            await Task.CompletedTask;
        }
        
        private SocketTextChannel GetChannel(ulong id)
        {
            var channel = Client.GetChannel(id);
            if (channel.GetChannelType() != ChannelType.Text)
                return null;
            var textChannel = (SocketTextChannel)channel;
            return textChannel;
        }
        
        public void SetStatus(bool force = false)
        {
            var playerList = API.Features.Player.List;
            var players = string.Join("\n", playerList.Select(player => "- " + player.Nickname));
            var description = force ? players : !Round.IsEnded && Round.IsStarted ? players == "" ? "Waiting for players." : players : "Waiting for players.";
            var embedBuilder = new EmbedBuilder()
                .WithTitle("Silly SCP Member List")
                .WithColor(Color.Blue)
                .WithDescription(
                    description
                );
            SetMessage(_statusChannel, 1280910252325339311, embedBuilder.Build());
            SetCustomStatus(force);
        }

        private bool _messageCooldown;

        private void SetMessage(SocketTextChannel channel, ulong messageId, Embed embed)
        {
            var message = channel.GetMessageAsync(messageId).Result;
            
            try
            {
                if (message.Embeds != null && embed.Description == message.Embeds.FirstOrDefault()?.Description) return;
                channel.ModifyMessageAsync(
                    message.Id,
                    msg =>
                    {
                        msg.Embed = embed;
                        msg.Content = "";
                    }
                );
                _messageCooldown = false;
            }
            catch
            {
                if(!_messageCooldown)
                {
                    _messageCooldown = true;
                    Timing.CallDelayed(10, () =>
                    {
                        if (!_messageCooldown) return;
                        SetMessage(channel, messageId, embed);
                    });
                }
            }
        }
        
        private bool _statusCooldown;

        private void SetCustomStatus(bool force = false)
        {
            var status = (!Round.IsEnded && Round.IsStarted) || force
                ? $"{API.Features.Server.PlayerCount}/{API.Features.Server.MaxPlayerCount} players"
                : "Waiting for players.";
            try
            {
                if (Client.Activity != null && Client.Activity?.ToString().Trim() == status) return;
                Client.SetCustomStatusAsync(status);
                _statusCooldown = false;
            }
            catch
            {
                if (!_statusCooldown)
                {
                    _statusCooldown = true;
                    Timing.CallDelayed(10, () =>
                    {
                        if (!_statusCooldown) return;
                        SetCustomStatus();
                    });
                }
            }
        }
    }
}