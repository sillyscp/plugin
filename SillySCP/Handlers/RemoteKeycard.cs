using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
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
            ev.CanOpen = ev.Player.HasDoorPermission(ev.Door, DoorPermissionCheck.FullInventory);
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