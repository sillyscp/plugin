using System;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using PlayerRoles;
using Round = PluginAPI.Core.Round;

namespace SillySCP
{
    public class ExiledEvents
    {
        public void OnWaitingForPlayers()
        {
            Plugin.Instance.Scp106 = null;
            Plugin.Instance.SetStatus();
        }

        public void OnPlayerLeave(LeftEventArgs ev)
        {
            if(Plugin.Instance.Volunteers == null)
                return;
            var volunteeredScp = Plugin.Instance.Volunteers.FirstOrDefault((v) => v.Players.Contains(ev.Player));
            if (volunteeredScp != null) volunteeredScp.Players.Remove(ev.Player);
            if(!String.IsNullOrEmpty(ev.Player.Nickname) && !Round.IsRoundEnded && Round.IsRoundStarted) Plugin.Client.GetGuild(1279504339248877588).GetTextChannel(1294978305253970002).SendMessageAsync($"Player `{ev.Player.Nickname}` has left the server");
            if (!Player.List.Any() && Round.IsRoundStarted)
            {
                Round.Restart();
            }
        }

        public void OnPlayerDamageWindow(DamagingWindowEventArgs ev)
        {
            if (ev.Window.Type.ToString() == "Scp079Trigger" && Plugin.Instance.ChosenEvent == "Lights Out")
            {
                Plugin.Instance.RoundEvents.ResetLightsOut();
                Plugin.Instance.ChosenEvent = null;
            }
        }

        public void OnSpawned(SpawnedEventArgs ev)
        {
            if(ev.Player.IsHuman && Plugin.Instance.ChosenEvent == "Lights Out")
            { 
                ev.Player.AddItem(ItemType.Flashlight);
            }
        }

        public void OnPlayerDying(DyingEventArgs ev)
        {
            var text = "";
            if(ev.Attacker != null && ev.Player != ev.Attacker)
            {
                var cuffed = false;
                if(ev.Player.Role == RoleTypeId.ClassD || ev.Player.Role == RoleTypeId.Scientist || ev.Player.Role == RoleTypeId.FacilityGuard)
                {
                    cuffed = ev.Player.IsCuffed;
                }
                text += $"Player `{ev.Player.Nickname}` (`{ev.Player.Role.Name}`){(cuffed ? " **(was cuffed)**" : "")} has been killed by `{ev.Attacker.Nickname}` as `{ev.Attacker.Role.Name}`";
                Plugin.Client.GetGuild(1279504339248877588).GetTextChannel(1296011257006002207).SendMessageAsync(text);
            }
        }

        public void OnRoundEnded(RoundEndedEventArgs _)
        {
            var discMessage = "Round has ended with the following people:\n```";
            discMessage += string.Join("\n", Player.List.Select(player => player.Nickname));
            discMessage += "```";
            Plugin.Client.GetGuild(1279504339248877588).GetTextChannel(1294978305253970002).SendMessageAsync(discMessage);
        }
    }
}