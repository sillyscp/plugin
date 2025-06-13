using CustomPlayerEffects;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp914Events;
using LabApi.Events.Handlers;
using LabApi.Features.Extensions;
using LabApi.Features.Wrappers;
using MapGeneration;
using PlayerRoles;
using Scp914;
using SecretAPI.Extensions;
using SecretAPI.Features;
using SillySCP.API.Extensions;
using SillySCP.API.Features;
using SillySCP.API.Modules;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SillySCP.Handlers
{
    public class Player : IRegister
    {
        public static Player Instance { get; private set; }
        
        public void TryRegister()
        {
            Instance = this;
            PlayerEvents.Spawned += OnSpawned;
            PlayerEvents.Dying += ScpDeathHandler;
            Scp914Events.ProcessingPlayer += OnUpgradingPlayer;
            Scp914Events.ProcessingInventoryItem += OnScp914UpgradeInv;
            PlayerEvents.Escaping += OnEscaping;
            PlayerEvents.Kicking += OnKickingPlayer;
            
            PlayerEvents.Death += OnDeath;
            PlayerEvents.ChangedSpectator += OnChangedSpectator;
        }

        public void TryUnregister()
        {
            PlayerEvents.Spawned -= OnSpawned;
            PlayerEvents.Dying -= ScpDeathHandler;
            Scp914Events.ProcessingPlayer -= OnUpgradingPlayer;
            Scp914Events.ProcessingInventoryItem -= OnScp914UpgradeInv;
            PlayerEvents.Escaping -= OnEscaping;
            PlayerEvents.Kicking -= OnKickingPlayer;
            
            PlayerEvents.Death -= OnDeath;
            PlayerEvents.ChangedSpectator -= OnChangedSpectator;
        }

        private void OnDeath(PlayerDeathEventArgs ev)
        {
            PlayerStat stat = ev.Player.FindOrCreatePlayerStat();
            stat.SpectatingKillsDisplay.Update();
        }

        private void OnChangedSpectator(PlayerChangedSpectatorEventArgs ev)
        {
            PlayerStat stat = ev.Player.FindOrCreatePlayerStat();
            stat.SpectatingKillsDisplay.Update();
        }

        private void OnKickingPlayer(PlayerKickingEventArgs ev)
        {
            if (!ev.Reason.Contains("AFK")) return;

            RoleTypeId.Tutorial.GetRandomSpawnPosition(out Vector3 position, out float _);
            
            bool allowed = !(Vector3.Distance(position, ev.Player.Position) > 11f || ev.Player.IsSpeaking);

            ev.IsAllowed = allowed;

            if (!allowed) return;
            LabApi.Features.Wrappers.Player player = LabApi.Features.Wrappers.Player.List.Where(player => player.Role == RoleTypeId.Spectator).GetRandomValue();
            if(player == null) return;
            player.Role = ev.Player.Role;
            player.Position = ev.Player.Position;
        }

        private void OnUpgradingPlayer(Scp914ProcessingPlayerEventArgs ev)
        {
            if (ev.KnobSetting == Scp914KnobSetting.Rough && ev.Player.CurrentItem == null)
            {
                ev.IsAllowed = false;
                Room randomRoom = Room.Get(FacilityZone.LightContainment)
                    .Where(r => r.GameObject.GetStrippedName() is not "LCZ_330" and not "LCZ_914" and not "LCZ_Armory" and not "LCZ_173")
                    .GetRandomValue();
                
                ev.Player.Position = new (randomRoom.Position.x, randomRoom.Position.y + 1, randomRoom.Position.z);
                if (ev.Player.IsHuman)
                {
                    if (ev.Player.Health <= 25)
                    {
                        ev.Player.Kill("Killed whilst trying to escape SCP-914...");
                        return;
                    }
                    ev.Player.EnableEffect<Disabled>(1, 10);
                }
                else
                {
                    ev.Player.EnableEffect<Flashed>(1, 10);
                }

                ev.Player.Health *= 0.75f;
            }
        }

        private void ScpDeathHandler(PlayerDyingEventArgs ev)
        {
            if (!ev.Player.Role.IsScp()) return;
            List<LabApi.Features.Wrappers.Player> scps = LabApi.Features.Wrappers.Player.List.Where(p => p.IsSCP).ToList();
            if (scps.Count == 1 && scps.First().Role == RoleTypeId.Scp079 &&
                !VolunteerSystem.ReadyVolunteers)
            {
                Scp079Recontainment.Recontain();
            }
        }
        
        // private void OnUsingItemCompleted(PlayerItem ev)
        // {
        //     Vector3 pos = ev.Player.Position;
        //     StatusEffectBase effectNormal = ev.Player.GetEffect(EffectType.Scp207);
        //     StatusEffectBase effectAnti = ev.Player.GetEffect(EffectType.AntiScp207);
        //     if (!effectNormal.IsEnabled || effectAnti.IsEnabled) return;
        //     if (ev.Item.Type == ItemType.SCP207 && effectAnti.Intensity > 1)
        //     {
        //         byte intensity = effectAnti.Intensity;
        //         Map.Explode(pos, ProjectileType.FragGrenade, ev.Player);
        //         if (intensity == 2)
        //         {
        //             Map.Explode(pos, ProjectileType.FragGrenade, ev.Player);
        //         }
        //         else if (intensity == 3)
        //         {
        //             Map.Explode(pos, ProjectileType.FragGrenade, ev.Player);
        //             Map.Explode(pos, ProjectileType.FragGrenade, ev.Player);
        //         }
        //     }
        //
        //     if (ev.Item.Type == ItemType.AntiSCP207 && effectNormal.Intensity > 1)
        //     {
        //         byte intensity = effectNormal.Intensity;
        //         Map.Explode(pos, ProjectileType.FragGrenade, ev.Player);
        //         if (intensity == 2)
        //         {
        //             Map.Explode(pos, ProjectileType.FragGrenade, ev.Player);
        //         }
        //         else if (intensity == 3)
        //         {
        //             Map.Explode(pos, ProjectileType.FragGrenade, ev.Player);
        //             Map.Explode(pos, ProjectileType.FragGrenade, ev.Player);
        //         }
        //     }
        // }

        private void OnEscaping(PlayerEscapingEventArgs ev)
        {
            if (ev.Player.Role == RoleTypeId.FacilityGuard && ev.Player.IsDisarmed)
            {
                ev.EscapeScenario = Escape.EscapeScenarioType.Custom;
                ev.IsAllowed = true;
                ev.NewRole = RoleTypeId.ChaosConscript;
            }
            if(ev.Player.IsNTF && ev.Player.IsDisarmed)
            {
                ev.EscapeScenario = Escape.EscapeScenarioType.Custom;
                ev.IsAllowed = true;
                ev.NewRole = RoleTypeId.ChaosConscript;
            }
            if(ev.Player.IsChaos && ev.Player.IsDisarmed)
            {
                ev.EscapeScenario = Escape.EscapeScenarioType.Custom;
                ev.IsAllowed = true;
                ev.NewRole = RoleTypeId.NtfPrivate;
            }
        }

        private void OnSpawned(PlayerSpawnedEventArgs ev)
        {
            switch (ev.Player.Role)
            {
                case RoleTypeId.ClassD:
                    ev.Player.AddItem(ItemType.Coin);
                    break;
                
                case RoleTypeId.Scientist:
                    ev.Player.AddItem(ItemType.Flashlight);
                    break;
            }
            
            if (ev.Player.Role == RoleTypeId.Tutorial && ev.Player.RemoteAdminAccess)
            {
                ev.Player.IsGodModeEnabled = true;
            } 
            if (ev.Player.Role != RoleTypeId.Tutorial && ev.Player.RemoteAdminAccess && ev.Player.IsGodModeEnabled)
            {
                ev.Player.IsGodModeEnabled = false;
            }
        }
        
        private void OnScp914UpgradeInv(Scp914ProcessingInventoryItemEventArgs ev)
        {
            if (ev.KnobSetting == Scp914KnobSetting.Fine && ev.Item.Type == ItemType.Coin)
            {
                int randomNum = Random.Range(1, 4);
                ev.Player.RemoveItem(ev.Player.CurrentItem!);
                ev.IsAllowed = false;
                switch (randomNum)
                {
                    case 1:
                    {
                        ev.Player.AddItem(ItemType.Flashlight);
                        break;
                    }
                    case 2:
                    {
                        ev.Player.AddItem(ItemType.Radio);
                        break;
                    }
                    case 3:
                    {
                        ev.Player.AddItem(ItemType.KeycardJanitor);
                        break;
                    }
                }
            }
        }
    }
}