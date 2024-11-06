using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp914;
using JetBrains.Annotations;
using MEC;
using PlayerRoles;
using PluginAPI.Enums;
using Scp914;
using UnityEngine;
using Features = Exiled.API.Features;
using Random = UnityEngine.Random;

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
            Exiled.Events.Handlers.Player.Verified += OnPlayerVerified;
            Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;
            Exiled.Events.Handlers.Player.ChangingSpectatedPlayer += OnChangingSpectatedPlayer;
            Exiled.Events.Handlers.Scp914.UpgradingInventoryItem += OnScp914UpgradeInv;
            Exiled.Events.Handlers.Scp914.UpgradingPickup += OnScp914UpgradePickup;
        }

        public void Unsubscribe()
        {
            Exiled.Events.Handlers.Player.Left -= OnPlayerLeave;
            Exiled.Events.Handlers.Player.PlayerDamageWindow -= OnPlayerDamageWindow;
            Exiled.Events.Handlers.Player.Spawned -= OnSpawned;
            Exiled.Events.Handlers.Player.Dying -= OnPlayerDying;
            Exiled.Events.Handlers.Player.Died -= OnPlayerDead;
            Exiled.Events.Handlers.Player.Verified -= OnPlayerVerified;
            Exiled.Events.Handlers.Player.ChangingRole -= OnChangingRole;
            Exiled.Events.Handlers.Player.ChangingSpectatedPlayer -= OnChangingSpectatedPlayer;
            Exiled.Events.Handlers.Scp914.UpgradingInventoryItem -= OnScp914UpgradeInv;
            Exiled.Events.Handlers.Scp914.UpgradingPickup -= OnScp914UpgradePickup;
        }

        private void OnPlayerVerified(VerifiedEventArgs ev)
        {
            if (!Round.IsEnded && Round.IsStarted && ev.Player.Role == RoleTypeId.Spectator)
            {
                Timing.RunCoroutine(Plugin.Instance.RespawnTimer(ev.Player));
            }
            if(!String.IsNullOrEmpty(ev.Player.Nickname) && Round.IsStarted)
            {
                Plugin.Client.GetGuild(1279504339248877588).GetTextChannel(1294978305253970002)
                    .SendMessageAsync($"Player `{ev.Player.Nickname}` has joined the server");
                Plugin.Instance.SetStatus();
            }
        }
        
        private void OnPlayerLeave(LeftEventArgs ev)
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

        private void OnPlayerDamageWindow(DamagingWindowEventArgs ev)
        {
            if (ev.Window.Type.ToString() == "Scp079Trigger" && Plugin.Instance.ChosenEvent == "Lights Out")
            {
                Plugin.Instance.RoundEvents.ResetLightsOut();
                Plugin.Instance.ChosenEvent = null;
            }
        }

        private void OnSpawned(SpawnedEventArgs ev)
        {
            if(ev.Player.IsHuman && Plugin.Instance.ChosenEvent == "Lights Out")
            { 
                ev.Player.AddItem(ItemType.Flashlight);
            }
            if (!ev.Player.IsAlive && Round.IsStarted)
            {
                Timing.RunCoroutine(Plugin.Instance.RespawnTimer(ev.Player));
            }

            if (ev.Player.IsAlive)
            {
                if (ev.Player.Role == RoleTypeId.Scp106 && !ev.Player.DoNotTrack)
                    Plugin.Instance.Scp106 = ev.Player;
                ev.Player.ShowHint("", int.MaxValue);
                var playerStats = Plugin.Instance.FindPlayerStat(ev.Player);
                if (playerStats != null) playerStats.Spectating = null;
            }

            if (ev.Player.Role == RoleTypeId.ClassD)
            {
                var random = Random.Range(1, 1_000_000);
                if (random == 1)
                {
                    ev.Player.Scale = new Vector3(ev.Player.Scale.x * 2, ev.Player.Scale.y, ev.Player.Scale.z);
                }
            }
        }

        private void OnPlayerDying(DyingEventArgs ev)
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

        private void OnPlayerDead(DiedEventArgs ev)
        {
            Timing.RunCoroutine(Plugin.Instance.RespawnTimer(ev.Player));
            if (ev.DamageHandler.Type == Exiled.API.Enums.DamageType.PocketDimension)
            {
                var scp106 = Plugin.Instance.Scp106;
                if(scp106 == null) scp106 = Features.Player.List.FirstOrDefault((p) => p.Role == RoleTypeId.Scp106);
                Plugin.Instance.UpdateKills(scp106, true);
            }
            if (ev.Attacker == null)
                return;
            if (ev.Player == ev.Attacker)
                return;
            Plugin.Instance.UpdateKills(ev.Attacker, ev.Attacker.IsScp);
        }
        
        private void OnChangingRole(ChangingRoleEventArgs ev)
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
        
        private void OnChangingSpectatedPlayer(ChangingSpectatedPlayerEventArgs ev)
        {
            if (ev.NewTarget == null)
                return;
            var playerStats = Plugin.Instance.FindOrCreatePlayerStat(ev.Player);
            playerStats.Spectating = ev.NewTarget;
        }
        
        private void OnScp914UpgradeInv(UpgradingInventoryItemEventArgs ev)
        {
            if (ev.KnobSetting == Scp914KnobSetting.Fine && ev.Item.Type == ItemType.Coin)
            {
                var randomNum = Random.Range(1, 3);
                switch (randomNum)
                {
                    case 1:
                    {
                        ev.Item.Destroy();
                        ev.Player.AddItem(ItemType.Flashlight);
                        break;
                    }
                    case 2:
                    {
                        ev.Item.Destroy();
                        ev.Player.AddItem(ItemType.Radio);
                    }
                        break;
                    case 3:
                    {
                        ev.Item.Destroy();
                        ev.Player.AddItem(ItemType.KeycardJanitor);
                        break;
                    }
                }
            }
        }

        private void OnScp914UpgradePickup(UpgradingPickupEventArgs ev)
        {
            if (ev.KnobSetting == Scp914KnobSetting.Fine && ev.Pickup.Type == ItemType.Coin)
            {
                var randomNum = Random.Range(1, 3);
                switch (randomNum)
                {
                    case 1:
                    {
                        ev.Pickup.Destroy();
                        Pickup.CreateAndSpawn(ItemType.Flashlight, ev.OutputPosition, Quaternion.identity);
                        break;
                    }
                    case 2:
                    {
                        ev.Pickup.Destroy();
                        Pickup.CreateAndSpawn(ItemType.Radio, ev.OutputPosition, Quaternion.identity);
                        break;
                    }
                    case 3:
                    {
                        ev.Pickup.Destroy();
                        Pickup.CreateAndSpawn(ItemType.KeycardJanitor, ev.OutputPosition, Quaternion.identity);
                        break;
                    }
                }
            }
        }
    }
}