using Discord;
using Discord.WebSocket;
using Exiled.API.Features;
using MEC;
using SillySCP.API;

namespace SillySCP.Handlers
{
    public class DiscordBot : IInittable
    {
        public static DiscordBot Instance { get; private set; }
        
        private DiscordSocketClient Client { get; set; }

        private SocketTextChannel _statusChannel;
        public SocketTextChannel DeathChannel;
        public SocketTextChannel ConnectionChannel;
        public SocketTextChannel ScpSwapChannel;

        public SocketGuild Guild;

        public void Init()
        {
            Instance = this;
            Task.Run(StartClient);
        }

        public void Unregister()
        {
            Task.Run(StopClient);
        }

        private Task DiscLog(LogMessage msg)
        {
            PluginAPI.Core.Log.Info(msg.ToString());
            return Task.CompletedTask;
        }

        public async Task StartClient()
        {
            DiscordSocketConfig config = new()
            {
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages,
                LogLevel = LogSeverity.Error
            };
            Client = new(config);
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
            ScpSwapChannel = GetChannel(1306013611851907082);
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
            var playerList = Exiled.API.Features.Player.List;
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
            Task.Run(async () =>
            {
                var message = await channel.GetMessageAsync(messageId);

                try
                {
                    if (message.Embeds != null &&
                        embed.Description == message.Embeds.FirstOrDefault()?.Description) return;
                    await channel.ModifyMessageAsync(
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
                    if (!_messageCooldown)
                    {
                        _messageCooldown = true;
                        Timing.CallDelayed(10, () =>
                        {
                            if (!_messageCooldown) return;
                            SetMessage(channel, messageId, embed);
                        });
                    }
                }
            });
        }
        
        private bool _statusCooldown;

        private void SetCustomStatus(bool force = false)
        {
            var status = (!Round.IsEnded && Round.IsStarted) || force
                ? $"{Exiled.API.Features.Server.PlayerCount}/{Exiled.API.Features.Server.MaxPlayerCount} players"
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