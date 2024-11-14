using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using PlayerRoles;
using SillySCP.API.Interfaces;

namespace SillySCP.Handlers;

public class DiscordLogHandler : IRegisterable
{
    public void Init()
    {
        Exiled.Events.Handlers.Player.Verified += OnPlayerVerified;
        Exiled.Events.Handlers.Player.Left += OnPlayerLeave;
        Exiled.Events.Handlers.Player.Dying += OnPlayerDying;
        Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
        Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
        Exiled.Events.Handlers.Server.RestartingRound += OnRoundRestart;
        Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
    }

    public void Unregister()
    {
        Exiled.Events.Handlers.Player.Verified -= OnPlayerVerified;
        Exiled.Events.Handlers.Player.Left -= OnPlayerLeave;
        Exiled.Events.Handlers.Player.Dying -= OnPlayerDying;
        Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
        Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
        Exiled.Events.Handlers.Server.RestartingRound -= OnRoundRestart;
        Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
    }
    
    private void OnPlayerVerified(VerifiedEventArgs ev)
    {
        if (!string.IsNullOrEmpty(ev.Player.Nickname) && Round.IsStarted)
        {
            DiscordBot.Instance.ConnectionChannel
                .SendMessageAsync($"Player `{ev.Player.Nickname}` (`{ev.Player.UserId}`) has joined the server");
            DiscordBot.Instance.SetStatus();
        }
    }
    
    public void OnPlayerLeave(LeftEventArgs ev)
    {
        
        if (!Round.IsEnded && Round.IsStarted) 
            DiscordBot.Instance.SetStatus();
        if (!string.IsNullOrEmpty(ev.Player.Nickname) && !Round.IsEnded && Round.IsStarted)
            DiscordBot.Instance.ConnectionChannel.SendMessageAsync($"Player `{ev.Player.Nickname}` (`{ev.Player.UserId}`) has left the server");
    }
    
    private void OnPlayerDying(DyingEventArgs ev)
    {
        string text = "";
        if (ev.Attacker != null && ev.Player != ev.Attacker)
        {
            var cuffed = false;
            if(ev.Player.Role == RoleTypeId.ClassD || ev.Player.Role == RoleTypeId.Scientist || ev.Player.Role == RoleTypeId.FacilityGuard)
            {
                cuffed = ev.Player.IsCuffed;
            }
            text += $"Player `{ev.Player.Nickname}` (`{ev.Player.Role.Name}`){(cuffed ? " **(was cuffed)**" : "")} has been killed by `{ev.Attacker.Nickname}` as `{ev.Attacker.Role.Name}`";
            DiscordBot.Instance.DeathChannel.SendMessageAsync(text);
        }
    }

    private void OnRoundEnded(RoundEndedEventArgs _)
    {
        var discMessage = "Round has ended with the following people:\n```";
        discMessage += string.Join("\n", Exiled.API.Features.Player.List.Select(player => $"{player.Nickname} ({player.UserId})"));
        discMessage += "```";
        DiscordBot.Instance.ConnectionChannel.SendMessageAsync(discMessage);
    }

    private void OnRoundStarted()
    {
        DiscordBot.Instance.SetStatus(true);

        var message = "Round has started with the following people:\n```";
        message += string.Join("\n", Exiled.API.Features.Player.List.Select(player => $"{player.Nickname} ({player.UserId})"));
        message += "```";
        DiscordBot.Instance.ConnectionChannel.SendMessageAsync(message);
        DiscordBot.Instance.DeathChannel.SendMessageAsync("New round");
    }

    private void OnRoundRestart()
    {
        DiscordBot.Instance.SetStatus();
    }
    
    private void OnWaitingForPlayers()
    {
        DiscordBot.Instance.SetStatus();
    }
}