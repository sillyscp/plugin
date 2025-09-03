using System.Net.Http;
using CustomPlayerEffects;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp096Events;
using LabApi.Events.Arguments.Scp173Events;
using LabApi.Events.Arguments.Scp914Events;
using LabApi.Events.Handlers;
using LabApi.Features.Extensions;
using LabApi.Features.Wrappers;
using MapGeneration;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp096;
using PlayerRoles.PlayableScps.Scp106;
using PlayerStatsSystem;
using Scp914;
using SecretAPI.Extensions;
using SecretAPI.Features;
using SillySCP.API.Extensions;
using SillySCP.API.Features;
using SillySCP.API.Modules;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;
using Random = UnityEngine.Random;

namespace SillySCP.Handlers
{
    public class Player : IRegister
    {
        public static Player Instance { get; private set; }
        
        // my ass is not putting the bee movie script in code.
        public static string BeeMovieScript { get; private set; }
        
        public void TryRegister()
        {
            Instance = this;
            PlayerEvents.Spawned += OnSpawned;
            PlayerEvents.Dying += ScpDeathHandler;
            Scp914Events.ProcessingPlayer += OnUpgradingPlayer;
            Scp914Events.ProcessingInventoryItem += OnScp914UpgradeInv;
            PlayerEvents.Escaping += OnEscaping;
            PlayerEvents.Kicking += OnKickingPlayer;
            
            // PlayerEvents.LeavingPocketDimension += OnLeavingPocketDimension;
            
            PlayerEvents.Death += OnDeath;
            PlayerEvents.ChangedSpectator += OnChangedSpectator;
            
            PlayerEvents.ReceivedLoadout += OnReceivedLoadout;

            Scp173Events.AddingObserver += OnAddingObserver;

            Scp096Events.AddingTarget += OnAddingTarget;

            PlayerEvents.ValidatedVisibility += OnValidatedVisibility;

            PlayerEvents.SpawningRagdoll += OnSpawningRagdoll;

            if(BeeMovieScript == null)
                Task.Run(FetchScript);
        }

        public void TryUnregister()
        {
            PlayerEvents.Spawned -= OnSpawned;
            PlayerEvents.Dying -= ScpDeathHandler;
            Scp914Events.ProcessingPlayer -= OnUpgradingPlayer;
            Scp914Events.ProcessingInventoryItem -= OnScp914UpgradeInv;
            PlayerEvents.Escaping -= OnEscaping;
            PlayerEvents.Kicking -= OnKickingPlayer;
            
            // PlayerEvents.LeavingPocketDimension -= OnLeavingPocketDimension;
            
            PlayerEvents.Death -= OnDeath;
            PlayerEvents.ChangedSpectator -= OnChangedSpectator;
            
            PlayerEvents.ReceivedLoadout -= OnReceivedLoadout;
            
            Scp173Events.AddingObserver -= OnAddingObserver;

            Scp096Events.AddingTarget -= OnAddingTarget;

            PlayerEvents.ValidatedVisibility -= OnValidatedVisibility;

            PlayerEvents.SpawningRagdoll -= OnSpawningRagdoll;
        }

        private static async Task FetchScript()
        {
            using HttpClient client = new();

            using HttpResponseMessage message = await client.GetAsync(
                "https://gist.githubusercontent.com/MattIPv4/045239bc27b16b2bcf7a3a9a4648c08a/raw/2411e31293a35f3e565f61e7490a806d4720ea7e/bee%2520movie%2520script");

            BeeMovieScript = await message.Content.ReadAsStringAsync();
            BeeMovieScript = BeeMovieScript.Replace("\n", " - ").Substring(0, 10000);
        }

        private static void OnSpawningRagdoll(PlayerSpawningRagdollEventArgs ev)
        {
            bool anyRagdoll = Ragdoll.List.Any(ragdoll =>
                ragdoll.DamageHandler is CustomReasonDamageHandler handler &&
                handler.RagdollInspectText == BeeMovieScript);

            if (anyRagdoll && Random.Range(0, 25) != 0) return;

            ev.IsAllowed = false;

            Ragdoll.SpawnRagdoll(ev.Player, new CustomReasonDamageHandler(BeeMovieScript));
        }

        private void OnValidatedVisibility(PlayerValidatedVisibilityEventArgs ev)
        {
            if (ev.Player.RoleBase is not Scp096Role role || role.StateController.RageState != Scp096RageState.Enraged)
                return;

            if (!ev.Target.IsSCP)
                return;

            ev.IsVisible = true;
        }

        private void OnAddingTarget(Scp096AddingTargetEventArgs ev)
        {
            if (ev.Target.Role != RoleTypeId.Tutorial)
                return;

            ev.IsAllowed = false;
        }

        private void OnAddingObserver(Scp173AddingObserverEventArgs ev)
        {
            if (ev.Target.Role != RoleTypeId.Tutorial)
                return;

            ev.IsAllowed = false;
        }

        private void OnReceivedLoadout(PlayerReceivedLoadoutEventArgs ev)
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
        }

        private void OnLeavingPocketDimension(PlayerLeavingPocketDimensionEventArgs ev)
        {
            if (!ev.IsSuccessful) return;
            FacilityZone zone = Room.GetRoomAtPosition(ev.Player.GetEffect<PocketCorroding>()!.CapturePosition.Position)!.Zone;
            Vector3[] exits = Scp106PocketExitFinder.GetPosesForZone(zone).Select(x => x.position).ToArray();
            LabApi.Features.Wrappers.Player[] players = LabApi.Features.Wrappers.Player.List.Where(x => x.Zone == zone && x.Team.GetFaction() != ev.Player.Team.GetFaction()).ToArray();
            Vector3 bestExit = Scp106PocketExitFinder.GetBestExitPosition(ev.Player.RoleBase as IFpcRole);
            bool shouldSpawn = true;
            foreach (LabApi.Features.Wrappers.Player player in players)
            {
                if (!shouldSpawn) break;
                if(CheckIfNear(bestExit, player.Position)) shouldSpawn = false;
            }
        
            if (shouldSpawn)
            {
                ev.Player.Position = bestExit;
                return;
            }
            foreach (Vector3 exit in exits)
            {
                bool canSpawn = true;
                foreach (LabApi.Features.Wrappers.Player player in players)
                {
                    if (!canSpawn) break;
                    if (CheckIfNear(exit, player.Position)) canSpawn = false;
                }
        
                if (canSpawn)
                {
                    ev.Player.Position = exit;
                    break;
                }
            }
        }
        
        private bool CheckIfNear(Vector3 position, Vector3 position2) => Vector3.Distance(position, position2) <= 15f;

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

            bool allowed = Vector3.Distance(ev.Player.Position, position) > 11;

            ev.IsAllowed = allowed;

            if (!allowed || ev.Player.Role == RoleTypeId.Tutorial) return;
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