using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Enums;
using LabApi.Features.Wrappers;
using MapGeneration;
using PlayerRoles;
using SecretAPI.Features;
using SillySCP.API.Features;

namespace SillySCP.Handlers;

public class Scp079Handler : IRegister
{
    public void TryRegister()
    {
        PlayerEvents.InteractingDoor += OnOpenDoor;
    }

    public void TryUnregister()
    {
        PlayerEvents.InteractingDoor -= OnOpenDoor;
    }

    public static void OnOpenDoor(PlayerInteractingDoorEventArgs ev)
    {
        if (ev.Player.Role != RoleTypeId.Scp079)
            return;

        Room room = ev.Door.Rooms.FirstOrDefault(r => r.Name == RoomName.LczArmory);

        if (room == null)
            return;

        if (!VolunteerSystem.ReadyVolunteers)
            return;

        ev.CanOpen = false;
    }
}