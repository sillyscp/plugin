using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using MEC;
using PlayerRoles;
using PluginAPI.Events;
using Respawning;
using Map = Exiled.API.Features.Map;
using Player = Exiled.API.Features.Player;
using Respawn = Exiled.API.Features.Respawn;
using Round = PluginAPI.Core.Round;
using Server = Exiled.API.Features.Server;
using UnityEngine;

namespace SillySCP
{
    public class Plugin : Plugin<Config>
    {
        public static Plugin Instance;
        public List<PlayerStat> PlayerStats;
        private EventHandler _handler;
        private ExiledEvents _exiledEvents;
        public RoundEvents RoundEvents;
        public static DiscordSocketClient Client;
        public PluginAPI.Core.Player Scp106;
        public List<Volunteers> Volunteers;
        public bool ReadyVolunteers;
        public string ChosenEvent;
        private SocketTextChannel _channel;

        public override void OnEnabled()
        {
            Instance = this;
            _handler = new EventHandler();
            _exiledEvents = new ExiledEvents();
            RoundEvents = new RoundEvents();
            EventManager.RegisterEvents(this, _handler);
            Exiled.Events.Handlers.Player.Left += _exiledEvents.OnPlayerLeave;
            Exiled.Events.Handlers.Server.WaitingForPlayers += _exiledEvents.OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RestartingRound += _exiledEvents.OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundEnded += _exiledEvents.OnRoundEnded;
            Exiled.Events.Handlers.Player.PlayerDamageWindow += _exiledEvents.OnPlayerDamageWindow;
            Exiled.Events.Handlers.Player.Spawned += _exiledEvents.OnSpawned;
            Exiled.Events.Handlers.Player.Dying += _exiledEvents.OnPlayerDying;
            Task.Run(StartClient);
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            EventManager.UnregisterEvents(this, _handler);
            Exiled.Events.Handlers.Player.Left -= _exiledEvents.OnPlayerLeave;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= _exiledEvents.OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RestartingRound -= _exiledEvents.OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundEnded -= _exiledEvents.OnRoundEnded;
            Exiled.Events.Handlers.Player.PlayerDamageWindow -= _exiledEvents.OnPlayerDamageWindow;
            Exiled.Events.Handlers.Player.Spawned -= _exiledEvents.OnSpawned;
            Exiled.Events.Handlers.Player.Dying -= _exiledEvents.OnPlayerDying;
            _handler = null;
            _exiledEvents = null;
            RoundEvents = null;
            Task.Run(StopClient);
            base.OnDisabled();
        }

        private static Task DiscLog(LogMessage msg)
        {
            PluginAPI.Core.Log.Info(msg.ToString());
            return Task.CompletedTask;
        }

        private static async Task StartClient()
        {
            var config = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages,
                LogLevel = LogSeverity.Error,
            };
            Client = new DiscordSocketClient(config);
            Client.Log += DiscLog;
            Client.Ready += Instance.Ready;
            await Client.LoginAsync(TokenType.Bot, Instance.Config.Token);
            await Client.StartAsync();
        }

        private static async Task StopClient()
        {
            await Client.StopAsync();
            await Client.LogoutAsync();
        }
        
        public void SetStatus(bool force = false)
        {
            var playerList = Player.List;
            var players = string.Join("\n", playerList.Select(player => "- " + player.Nickname));
            var description = force ? players : !Round.IsRoundEnded && Round.IsRoundStarted ? players == "" ? "Waiting for players." : players : "Waiting for players.";
            var embedBuilder = new EmbedBuilder()
                .WithTitle("Silly SCP Member List")
                .WithColor(Discord.Color.Blue)
                .WithDescription(
                    description
                );
            SetMessage(_channel, 1280910252325339311, embedBuilder.Build());
            SetCustomStatus(force);
        }

        private static SocketTextChannel GetChannel(ulong id)
        {
            var channel = Client.GetChannel(id);
            if (channel.GetChannelType() != ChannelType.Text)
                return null;
            var textChannel = (SocketTextChannel)channel;
            return textChannel;
        }

        private async Task Ready()
        {
            _channel = GetChannel(1279544677334253610);
            SetStatus();
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
            var status = (!Round.IsRoundEnded && Round.IsRoundStarted) || force
                ? $"{Server.PlayerCount}/{Server.MaxPlayerCount} players"
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

        public PlayerStat FindOrCreatePlayerStat(PluginAPI.Core.Player player)
        {
            var playerStat = FindPlayerStat(player);
            if (playerStat == null)
            {
                playerStat = new PlayerStat
                {
                    Player = player,
                    Kills = player.DoNotTrack ? (int?)null : 0,
                    ScpKills = player.DoNotTrack ? (int?)null : 0,
                    Spectating = null,
                };
                PlayerStats.Add(playerStat);
            }

            return playerStat;
        }
        
        public PlayerStat FindPlayerStat(PluginAPI.Core.Player player)
        {
            if(PlayerStats == null)
                PlayerStats = new List<PlayerStat>();
            return PlayerStats.Find((p) => p.Player == player);
        }

        public void UpdateKills(PluginAPI.Core.Player player, bool scp, bool positive = true)
        {
            if (player.DoNotTrack)
                return;
            var playerStat = FindOrCreatePlayerStat(player);
            if (positive && scp)
                playerStat.ScpKills++;
            else if (positive)
                playerStat.Kills++;
            else if (scp && playerStat.ScpKills > 0)
                playerStat.ScpKills--;
            else if (playerStat.Kills > 0 && !scp)
                playerStat.Kills--;
        }

        public IEnumerator<float> RespawnTimer(PluginAPI.Core.Player player)
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(1f);
                player = PluginAPI.Core.Player.GetPlayers().FirstOrDefault((p) => p.UserId == player.UserId);
                if (player == null || player.Role != RoleTypeId.Spectator)
                    break;
                var respawnTeam = Respawn.NextKnownTeam;
                var teamText = respawnTeam != SpawnableTeamType.None ? "<color=" + (respawnTeam == SpawnableTeamType.ChaosInsurgency ? "green>Chaos Insurgency" : "blue>Nine-Tailed Fox") + "</color>" : null;
                var timeUntilWave = Respawn.TimeUntilSpawnWave;
                timeUntilWave = teamText != null ? timeUntilWave : timeUntilWave.Add(System.TimeSpan.FromSeconds(Respawn.NtfTickets >= Respawn.ChaosTickets ? 17 : 13));
                var currentTime = $"{timeUntilWave.Minutes:D1}<size=22>M</size> {timeUntilWave.Seconds:D2}<size=22>S</size>";
                var playerStat = FindPlayerStat(player);
                var spectatingPlayerStat = playerStat != null && playerStat.Spectating != null ? FindPlayerStat(playerStat?.Spectating) : null;
                var kills = ((spectatingPlayerStat != null ? spectatingPlayerStat.Player.IsSCP ? spectatingPlayerStat.ScpKills : spectatingPlayerStat.Kills : 0) ?? 0).ToString();
                var spectatingKills =
                    spectatingPlayerStat != null
                        ? spectatingPlayerStat.Player.DoNotTrack == false ? string.IsNullOrEmpty(kills) ? "Unknown" : kills : "Unknown"
                        : "0";
                var text =
                    "<voffset=-4em><size=26>Respawning "
                    + (teamText != null ? "as " + teamText + " " : "")
                    + "in:\n</voffset>"
                    + currentTime
                    + "</size>"
                    + (playerStat?.Spectating != null ? "\n\nKill count: " + spectatingKills : "");
                player.ReceiveHint(text, 1.2f);
            }

            yield return 0;
        }

        public IEnumerator<float> DisableVolunteers()
        {
            ReadyVolunteers = true;
            yield return Timing.WaitForSeconds(180);
            ReadyVolunteers = false;
        }

        public IEnumerator<float> ChooseVolunteers(Volunteers volunteer)
        {
            yield return Timing.WaitForSeconds(15);
            volunteer = Volunteers.FirstOrDefault((v) => v.Replacement == volunteer.Replacement);
            if (volunteer == null)
                yield break;
            if (volunteer.Players.Count == 0) yield break;
            var replacementPlayer = volunteer.Players.GetRandomValue();
            replacementPlayer.Role.Set(volunteer.Replacement);
            Map.Broadcast(10, volunteer.Replacement.GetFullName() + " has been replaced!",
                Broadcast.BroadcastFlags.Normal, true);
            yield return 0;
        }

        public IEnumerator<float> HeartAttack()
        {
            while (!Round.IsRoundEnded && Round.IsRoundStarted)
            {
                yield return Timing.WaitForSeconds(5);
                var chance = Random.Range(1, 1_000_000);
                if (chance == 1)
                {
                    var player = Player.List.GetRandomValue();
                    player.EnableEffect(EffectType.CardiacArrest, 1, 3);
                }
            }

            yield return 0;
        }
    }
}
