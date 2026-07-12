using Interactables.Interobjects.DoorUtils;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Enums;
using LabApi.Features.Wrappers;
using SecretAPI.Enums;
using SecretAPI.Extensions;
using SecretAPI.Features;

namespace SillySCP.Handlers
{
    public class RemoteKeycard : IRegister
    {

        public void TryRegister()
        {
            PlayerEvents.InteractingDoor += OnInteractingDoor;
            PlayerEvents.UnlockingGenerator += OnUnlockingGenerator;
            PlayerEvents.InteractingLocker += OnInteractingLocker;
        }

        public void TryUnregister()
        {
            PlayerEvents.InteractingDoor -= OnInteractingDoor;
            PlayerEvents.UnlockingGenerator -= OnUnlockingGenerator;
            PlayerEvents.InteractingLocker -= OnInteractingLocker;
        }

        private static void OnInteractingDoor(PlayerInteractingDoorEventArgs ev)
        {
            if (ev.CanOpen) return;
            if (ev.Door.IsLocked) return;

            IDoorPermissionRequester door = ev.Door.Base;
            
            ev.CanOpen = ev.Player.HasDoorPermission(door, DoorPermissionCheck.FullInventory);

            if (ev.Door.DoorName is not (DoorName.EzGateA or DoorName.EzGateB)) return;
            
            IEnumerable<Item> keycards = ev.Player.Items.Where(item =>
                item.Base is IDoorPermissionProvider provider &&
                door.PermissionsPolicy.CheckPermissions(provider.GetPermissions(door))).ToArray();

            if (keycards.Count() >= 2)
                return;
                
            Item oneTimeUse = keycards.FirstOrDefault(item => item.Type == ItemType.SurfaceAccessPass);

            oneTimeUse?.DropItem().Destroy();
        }

        private static void OnUnlockingGenerator(PlayerUnlockingGeneratorEventArgs ev)
        {
            if (ev.Generator.IsUnlocked) return;
            ev.Generator.IsUnlocked = ev.Player.HasGeneratorPermission(ev.Generator, DoorPermissionCheck.FullInventory);
        }

        private static void OnInteractingLocker(PlayerInteractingLockerEventArgs ev)
        {
            if (ev.CanOpen) return;
            ev.CanOpen = ev.Player.HasLockerChamberPermission(ev.Chamber, DoorPermissionCheck.FullInventory);
        }
    }
}