using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using Features = Exiled.API.Features;

namespace SillySCP.Handlers
{
    public class Player
    {
        public void Init()
        {
            Exiled.Events.Handlers.Player.Left += OnPlayerLeave;
            Exiled.Events.Handlers.Player.PlayerDamageWindow += OnPlayerDamageWindow;
            Exiled.Events.Handlers.Player.Spawned += OnSpawned;
            Exiled.Events.Handlers.Player.Dying += OnPlayerDying;
            Exiled.Events.Handlers.Player.Died += OnPlayerDead;
            Exiled.Events.Handlers.Player.Joined += OnPlayerJoin;
            Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;
            Exiled.Events.Handlers.Player.ChangingSpectatedPlayer += OnChangingSpectatedPlayer;
        }

        public void Unsubscribe()
        {
            Exiled.Events.Handlers.Player.Left -= OnPlayerLeave;
            Exiled.Events.Handlers.Player.PlayerDamageWindow -= OnPlayerDamageWindow;
            Exiled.Events.Handlers.Player.Spawned -= OnSpawned;
            Exiled.Events.Handlers.Player.Dying -= OnPlayerDying;
            Exiled.Events.Handlers.Player.Died -= OnPlayerDead;
            Exiled.Events.Handlers.Player.Joined -= OnPlayerJoin;
            Exiled.Events.Handlers.Player.ChangingRole -= OnChangingRole;
            Exiled.Events.Handlers.Player.ChangingSpectatedPlayer -= OnChangingSpectatedPlayer;
        }
        
        public void OnPlayerLeave(LeftEventArgs ev)
        {
            var playerStats = Plugin.Instance.FindPlayerStat(ev.Player);
            if(playerStats != null) playerStats.Spectating = null;
            if(!Round.IsEnded && Round.IsStarted) Plugin.Instance.SetStatus();
            if(Plugin.Instance.Volunteers == null)
                return;
            var volunteeredScp = Plugin.Instance.Volunteers.FirstOrDefault((v) => v.Players.Contains(ev.Player));
            if (volunteeredScp != null) volunteeredScp.Players.Remove(ev.Player);
            if(!String.IsNullOrEmpty(ev.Player.Nickname) && !Round.IsEnded && Round.IsStarted) Plugin.Client.GetGuild(1279504339248877588).GetTextChannel(1294978305253970002).SendMessageAsync($"Player `{ev.Player.Nickname}` has left the server");
            if (!Features.Player.List.Any() && Round.IsStarted)
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
            if (ev.Player.Role == RoleTypeId.Spectator)
            {
                Timing.RunCoroutine(Plugin.Instance.RespawnTimer(ev.Player));
            }

            if (ev.Player.Role != RoleTypeId.Spectator || ev.Player.Role != RoleTypeId.None)
            {
                if (ev.Player.Role == RoleTypeId.Scp106 && ev.Player.DoNotTrack == false)
                    Plugin.Instance.Scp106 = ev.Player;
                ev.Player.ShowHint("", int.MaxValue);
                var playerStats = Plugin.Instance.FindPlayerStat(ev.Player);
                if (playerStats != null) playerStats.Spectating = null;
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

        public void OnPlayerDead(DiedEventArgs ev)
        {
            Timing.RunCoroutine(Plugin.Instance.RespawnTimer(ev.Player));
            if (ev.Attacker == null)
                return;
            if (ev.Player == ev.Attacker)
                return;
            Plugin.Instance.UpdateKills(ev.Attacker, ev.Attacker.IsScp);
        }

        public void OnPlayerJoin(JoinedEventArgs ev)
        {
            if (!Round.IsEnded && Round.IsStarted && ev.Player.Role == RoleTypeId.Spectator)
            {
                Timing.RunCoroutine(Plugin.Instance.RespawnTimer(ev.Player));
            }
            if(!String.IsNullOrEmpty(ev.Player.Nickname) && !Round.IsEnded && Round.IsStarted)
            {
                Plugin.Client.GetGuild(1279504339248877588).GetTextChannel(1294978305253970002)
                    .SendMessageAsync($"Player `{ev.Player.Nickname}` has joined the server");
                Plugin.Instance.SetStatus();
            }
        }

        public void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (ev.NewRole == RoleTypeId.Spectator) Timing.RunCoroutine(Plugin.Instance.RespawnTimer(ev.Player));
            if (ev.Player.IsScp && (ev.NewRole == RoleTypeId.Spectator || ev.NewRole == RoleTypeId.None) && Plugin.Instance.ReadyVolunteers)
            {
                Cassie.Clear();
                var volunteer = new Volunteers
                {
                    Replacement = ev.Player.Role,
                    Players = new List<Exiled.API.Features.Player>()
                };
                Plugin.Instance.Volunteers.Add(volunteer);
                if(!ev.Player.IsScp) return;
                if (ev.Player.Role == RoleTypeId.Scp0492) return;
                Map.Broadcast(10, $"{ev.Player.Role.Name} has left the game\nPlease run .volunteer {ev.Player.Role.Name.Split('-')[1]} to volunteer to be the SCP");
                Timing.RunCoroutine(Plugin.Instance.ChooseVolunteers(volunteer));
            }
        }
        
        public void OnChangingSpectatedPlayer(ChangingSpectatedPlayerEventArgs ev)
        {
            if (ev.NewTarget == null)
                return;
            var playerStats = Plugin.Instance.FindOrCreatePlayerStat(ev.Player);
            playerStats.Spectating = ev.NewTarget;
        }
    }
}