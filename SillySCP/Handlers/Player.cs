using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp106;
using Exiled.Events.EventArgs.Scp914;
using MEC;
using PlayerRoles;
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
            if(!String.IsNullOrEmpty(ev.Player.Nickname) && Round.IsStarted)
            {
                Plugin.Instance.Discord.ConnectionChannel
                    .SendMessageAsync($"Player `{ev.Player.Nickname}` (`{ev.Player.UserId}`) has joined the server");
                Plugin.Instance.Discord.SetStatus();
            }
        }
        
        private void OnPlayerLeave(LeftEventArgs ev)
        {
            var playerStats = Plugin.Instance.PlayerStatUtils.FindPlayerStat(ev.Player);
            if(playerStats != null) playerStats.Spectating = null;
            if(!Round.IsEnded && Round.IsStarted) Plugin.Instance.Discord.SetStatus();
            if(Plugin.Instance.Volunteers == null)
                return;
            var volunteeredScp = Plugin.Instance.Volunteers.FirstOrDefault(v => v.Players.Contains(ev.Player));
            if (volunteeredScp != null) volunteeredScp.Players.Remove(ev.Player);
            if(!String.IsNullOrEmpty(ev.Player.Nickname) && !Round.IsEnded && Round.IsStarted) Plugin.Instance.Discord.ConnectionChannel.SendMessageAsync($"Player `{ev.Player.Nickname}` (`{ev.Player.UserId}`) has left the server");
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
            if (Features.Player.List.Count(p => p.IsScp) == 2 && ev.Player.Role.Type == RoleTypeId.Scp079)
            {
                ev.Player.Role.Set(ev.OldRole);
                ev.Player.Broadcast(new Features.Broadcast("SCP-079 cannot spawn if there is 2 SCPs, it can spawn when above though"));
            }

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
                var playerStats = Plugin.Instance.PlayerStatUtils.FindPlayerStat(ev.Player);
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
                Plugin.Instance.Discord.DeathChannel.SendMessageAsync(text);
            }
        }

        private void OnPlayerDead(DiedEventArgs ev)
        {
            Timing.RunCoroutine(Plugin.Instance.RespawnTimer(ev.Player));
            if (ev.DamageHandler.Type == DamageType.PocketDimension)
            {
                var scp106 = Plugin.Instance.Scp106 ?? Features.Player.List.FirstOrDefault(p => p.Role == RoleTypeId.Scp106);
                Plugin.Instance.PlayerStatUtils.UpdateKills(scp106, true);
            }
            if (ev.Attacker == null)
                return;
            if (ev.Player == ev.Attacker)
                return;
            Plugin.Instance.PlayerStatUtils.UpdateKills(ev.Attacker, ev.Attacker.IsScp);
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
                    Players = new List<Features.Player>()
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
            var playerStats = Plugin.Instance.PlayerStatUtils.FindOrCreatePlayerStat(ev.Player);
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