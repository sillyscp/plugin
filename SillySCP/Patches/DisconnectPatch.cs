using System.Reflection.Emit;
using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
using LabApi.Features.Wrappers;
using MapGeneration;
using PlayerRoles;
using PlayerStatsSystem;
using SillySCP.API.Features;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace SillySCP.Patches
{
    [HarmonyPatch(typeof(CustomNetworkManager), nameof(CustomNetworkManager.OnServerDisconnect))]
    public static class DisconnectPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchStartForward(
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(PlayerStats), nameof(PlayerStats.DealDamage)))
                )
                .Advance(-6)
                .Insert(
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DisconnectPatch), nameof(OnBeforeDeath)))
                );
            
            return matcher.InstructionEnumeration();
        }

        public static void OnBeforeDeath(ReferenceHub hub)
        {
            Player player = Player.Get(hub);
            
            if (player is not { Role: RoleTypeId.Scp0492 })
                return;
            
            Scp049DataStore store = Scp049DataStore.ActiveStores.FirstOrDefault(store => store.ActivePlayers.Contains(player));
            if (store == null)
                return;

            Vector3 position;

            if (player.Room != null)
            {
                IEnumerable<Door> doors = player.Room.Doors.Where(door =>
                    door is not ElevatorDoor && !door.IsLocked && door.Permissions.HasFlag(DoorPermissionFlags.None));
                
                float closestDistance = float.MaxValue;
                Door closestDoor = null;
                foreach (Door door in doors)
                {
                    if (Vector3.Distance(door.Position, player.Position) >= closestDistance)
                        continue;
                    closestDistance = Vector3.Distance(door.Position, player.Position);
                    closestDoor = door;
                }
                
                position = closestDoor?.Position ?? player.Room.Position;
                position += Vector3.one;
                
                if (player.Room.Name is RoomName.EzGateA or RoomName.EzGateB or RoomName.HczCheckpointToEntranceZone)
                {
                    position = player.Position;
                }
            }
            else
            {
                position = player.Position;
            }
            
            LastKnownInformation info = new()
            {
                Position = position,
                Health = player.Health,
                MaxHealth = player.MaxHealth,
                HumeShield = player.HumeShield,
                MaxHumeShield = player.MaxHumeShield,
            };
            
            store.Leavers.Add(player.UserId, info);
            store.ActivePlayers.Remove(player);
        }
    }
}