using System.Collections.Generic;
using System.Linq;
using Exiled.Events.EventArgs.Server;
using MEC;
using Respawning;
using Features = Exiled.API.Features;

namespace SillySCP.Handlers
{
    public class Server
    {
        public void Init()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            Exiled.Events.Handlers.Server.RestartingRound += OnRoundRestart;
            Exiled.Events.Handlers.Server.RespawningTeam += OnRespawn;
        }

        public void Unsubscribe()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Server.RestartingRound -= OnRoundRestart;
            Exiled.Events.Handlers.Server.RespawningTeam -= OnRespawn;
        }
        
        public void OnWaitingForPlayers()
        {
            Plugin.Instance.Scp106 = null;
            Plugin.Instance.SetStatus();
        }

        public void OnRoundEnded(RoundEndedEventArgs _)
        {
            var highestKills = Plugin
                .Instance.PlayerStats.Where(p => !p.Player.IsScp && p.Kills > 0).OrderByDescending((p) => p.Kills)
                .ToList();
            var scpHighestKills = Plugin
                .Instance.PlayerStats.Where(p => p.Player.IsScp && p.ScpKills > 0).OrderByDescending((p) => p.ScpKills)
                .ToList();
            var highestKiller = highestKills.FirstOrDefault();
            var scpHighestKiller = scpHighestKills.FirstOrDefault();
            Features.Server.FriendlyFire = true;
            
            var normalMvpMessage = highestKiller != null ? $"Highest kills from a human for this round is {highestKiller.Player.Nickname} with {highestKiller.Kills}" : null;
            var scpMvpMessage = scpHighestKiller != null ? $"Highest kills from an SCP for this round is {scpHighestKiller.Player.Nickname} with {scpHighestKiller.ScpKills}" : null;
            var message = $"{normalMvpMessage}\n{scpMvpMessage}".Trim();
            
            Features.Map.Broadcast(
                10,
                message
            );
            
            
            var discMessage = "Round has ended with the following people:\n```";
            discMessage += string.Join("\n", Features.Player.List.Select(player => player.Nickname));
            discMessage += "```";
            Plugin.Client.GetGuild(1279504339248877588).GetTextChannel(1294978305253970002).SendMessageAsync(discMessage);
        }
        
        public void OnRoundStarted()
        {
            Features.Server.FriendlyFire = false;
            Plugin.Instance.PlayerStats = new List<PlayerStat>();
            Plugin.Instance.Volunteers = new List<Volunteers>();
            Timing.RunCoroutine(Plugin.Instance.DisableVolunteers());
            Plugin.Instance.SetStatus(true);
            // var eventRound = Plugin.Instance.RoundEvents.EventRound();
            // if (eventRound)
            // {
            //     var eventChosen = Plugin.Instance.RoundEvents.PlayRandomEvent();
            //     Plugin.Instance.ChosenEvent = eventChosen.Item1;
            //     Map.Broadcast(10, eventChosen.Item2);
            // }
            // else
            // {
            //     Plugin.Instance.ChosenEvent = null;
            // }

            var message = "Round has started with the following people:\n```";
            message += string.Join("\n", Features.Player.List.Select(player => player.Nickname));
            message += "```";
            Plugin.Client.GetGuild(1279504339248877588).GetTextChannel(1294978305253970002).SendMessageAsync(message);
            Timing.RunCoroutine(Plugin.Instance.HeartAttack());
        }

        public void OnRoundRestart()
        {
            Features.Server.FriendlyFire = false;
            Plugin.Instance.SetStatus();
        }

        public void OnRespawn(RespawningTeamEventArgs ev)
        {
            if (ev.NextKnownTeam == SpawnableTeamType.None)
                return;
            ev.Players.ForEach((p) =>
            {
                p.ShowHint("", int.MaxValue);
                var playerStats = Plugin.Instance.FindPlayerStat(p);
                if (playerStats == null) return;
                playerStats.Spectating = null;
            });
        }
    }
}