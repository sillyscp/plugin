using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp106;
using Exiled.Events.EventArgs.Scp914;
using MEC;
using PlayerRoles;
using PlayerRoles.RoleAssign;
using Scp914;
using SillySCP.API;
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
            Exiled.Events.Handlers.Player.Escaping += OnEscaping;
            Exiled.Events.Handlers.Scp106.Attacking += OnScp106Attacking;
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
            Exiled.Events.Handlers.Player.Escaping -= OnEscaping;
            Exiled.Events.Handlers.Scp106.Attacking -= OnScp106Attacking;
        }

        private void OnScp106Attacking(AttackingEventArgs ev)
        {
            if (!ev.Target.GetEffect(EffectType.Traumatized).IsEnabled)
            {
                ev.Target.EnableEffect(EffectType.PocketCorroding);
            }
        }

        private void OnEscaping(EscapingEventArgs ev)
        {
            if (ev.Player.Role.Type == RoleTypeId.FacilityGuard && ev.Player.IsCuffed)
            {
                ev.IsAllowed = true;
                ev.NewRole = RoleTypeId.ChaosConscript;
            }
        }

        private void OnPlayerVerified(VerifiedEventArgs ev)
        {
            if (!Round.IsEnded && Round.IsStarted && ev.Player.Role == RoleTypeId.Spectator)
            {
                Timing.RunCoroutine(Plugin.Instance.RespawnTimer(ev.Player));
            }
            if (!string.IsNullOrEmpty(ev.Player.Nickname) && Round.IsStarted)
            {
                DiscordBot.Instance.ConnectionChannel
                    .SendMessageAsync($"Player `{ev.Player.Nickname}` (`{ev.Player.UserId}`) has joined the server");
                DiscordBot.Instance.SetStatus();
            }
        }
        
        private void OnPlayerLeave(LeftEventArgs ev)
        {
            PlayerStat playerStats = PlayerStatUtils.FindPlayerStat(ev.Player);
            if (playerStats != null) playerStats.Spectating = null;
            if (!Round.IsEnded && Round.IsStarted) 
                DiscordBot.Instance.SetStatus();
            if (Plugin.Instance.Volunteers == null)
                return;
            Volunteers volunteeredScp = Plugin.Instance.Volunteers.FirstOrDefault(v => v.Players.Contains(ev.Player));
            if (volunteeredScp != null) volunteeredScp.Players.Remove(ev.Player);
            if (!string.IsNullOrEmpty(ev.Player.Nickname) && !Round.IsEnded && Round.IsStarted)
                DiscordBot.Instance.ConnectionChannel.SendMessageAsync($"Player `{ev.Player.Nickname}` (`{ev.Player.UserId}`) has left the server");
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
            if (Features.Player.List.Count(p => p.IsScp) is 1 or 2 && ev.Player.Role.Type == RoleTypeId.Scp079)
            {
                ev.Player.Role.Set(ev.OldRole.Team == Team.SCPs ? ev.OldRole.Type : ScpSpawner.NextScp);
                ev.Player.Broadcast(new("SCP-079 cannot at 1/2 scps."));
            }

            if (ev.Player.IsHuman && Plugin.Instance.ChosenEvent == "Lights Out")
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
                var playerStats = PlayerStatUtils.FindPlayerStat(ev.Player);
                if (playerStats != null) playerStats.Spectating = null;
            }

            if (ev.Player.Role == RoleTypeId.ClassD)
            {
                int random = Random.Range(1, 1_000_000);
                if (random == 1)
                {
                    ev.Player.Scale = new(ev.Player.Scale.x * 2, ev.Player.Scale.y, ev.Player.Scale.z);
                }
            }
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

        private void OnPlayerDead(DiedEventArgs ev)
        {
            Timing.RunCoroutine(Plugin.Instance.RespawnTimer(ev.Player));
            if (ev.DamageHandler.Type == DamageType.PocketDimension)
            {
                var scp106 = Plugin.Instance.Scp106 ?? Features.Player.List.FirstOrDefault(p => p.Role == RoleTypeId.Scp106);
                PlayerStatUtils.UpdateKills(scp106, true);
            }
            if (ev.Attacker == null)
                return;
            if (ev.Player == ev.Attacker)
                return;
            PlayerStatUtils.UpdateKills(ev.Attacker, ev.Attacker.IsScp);
        }

        private void OnEscaped(ChangingRoleEventArgs ev)
        {
            var inventoryToSpawn = new List<ItemType>();
            var inventoryToDrop = new List<ItemType>();
            var oldItems = ev.Player.Items.Select(i => i.Type).ToList();
            ev.Player.ClearInventory();

            oldItems
                .Where(item => item.IsScp() || item == ItemType.KeycardO5 || item == ItemType.MicroHID || item == ItemType.GunFRMG0)
                .ToList()
                .ForEach(AddItem);
            
            ev.Items.ForEach(AddItem);
            
            oldItems
                .Where(item => !inventoryToSpawn.Contains(item) && !inventoryToDrop.Contains(item))
                .ToList()
                .ForEach(AddItem);

            ev.Items.Clear();
            ev.Items.AddRange(inventoryToSpawn);

            Timing.CallDelayed(1f, () =>
            {
                foreach (var item in inventoryToDrop)
                {
                    Pickup.CreateAndSpawn(item, ev.Player.Position, Quaternion.identity);
                }
            });

            void AddItem(ItemType item)
            {
                if (inventoryToSpawn.Count >= 8)
                {
                    inventoryToDrop.Add(item);
                }
                else
                {
                    inventoryToSpawn.Add(item);
                }
            }
        }
        
        private void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (ev.Reason == SpawnReason.Escaped)
            {
                OnEscaped(ev);
            }
            if (ev.Player.IsScp && RoleExtensions.GetTeam(ev.NewRole) == Team.SCPs)
                DiscordBot.Instance.ScpSwapChannel.SendMessageAsync($"Player `{ev.Player.Nickname}` has swapped from `{ev.Player.Role.Name}` to `{ev.NewRole.GetFullName()}`");
            if (ev.NewRole == RoleTypeId.Spectator) Timing.RunCoroutine(Plugin.Instance.RespawnTimer(ev.Player));
            if (ev.Player.IsScp && (ev.NewRole == RoleTypeId.Spectator || ev.NewRole == RoleTypeId.None) && Plugin.Instance.ReadyVolunteers)
            {
                Cassie.Clear();
                var volunteer = new Volunteers
                {
                    Replacement = ev.Player.Role,
                    Players = new()
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
            var playerStats = PlayerStatUtils.FindOrCreatePlayerStat(ev.Player);
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

        public IEnumerator<float> StartNukeDamage(Features.Player player)
        {
            yield return Timing.WaitForSeconds(180);
            while(player.CurrentRoom.Type == RoomType.HczNuke)
            {
                player.Hurt(1f);
                yield return Timing.WaitForSeconds(1);
            }

            yield return 0;
        }
    }
}