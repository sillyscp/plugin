using System;
using System.Linq;
using Exiled.Events.EventArgs.Player;

namespace SillySCP
{
    public class ExiledEvents
    {
        public void OnWaitingForPlayers()
        {
            Plugin.Instance.Scp106 = null;
            Plugin.Instance.RoundStarted = false;
            Plugin.Instance.SetStatus();
        }

        public void OnPlayerLeave(LeftEventArgs ev)
        {
            if(Plugin.Instance.Volunteers == null)
                return;
            var volunteeredScp = Plugin.Instance.Volunteers.FirstOrDefault((v) => v.Players.Contains(ev.Player));
            if (volunteeredScp != null) volunteeredScp.Players.Remove(ev.Player);
            if(!String.IsNullOrEmpty(ev.Player.Nickname)) Plugin.Client.GetGuild(1279504339248877588).GetTextChannel(1294978305253970002).SendMessageAsync($"Player {ev.Player.Nickname} has left the server");
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

        public void OnPlayerDeath(DiedEventArgs ev)
        {
            var text = "";
            if(ev.Attacker != null)
            {
                text += $"Player {ev.Player.Nickname} has been killed by {ev.Attacker.Nickname}";
            }

            Plugin.Client.GetGuild(1279504339248877588).GetTextChannel(1296011257006002207).SendMessageAsync(text);
        }

        public void OnPlayerJoin(JoinedEventArgs ev)
        {
            if (!String.IsNullOrEmpty(ev.Player.Nickname)) Plugin.Client.GetGuild(1279504339248877588).GetTextChannel(1294978305253970002).SendMessageAsync($"Player {ev.Player.Nickname} has joined the server");
        }
    }
}