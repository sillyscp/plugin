using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Extensions;
using LabApi.Features.Wrappers;
using LogAssistant.Extensions;

namespace LogAssistant;

public class Events : CustomEventsHandler
{
    public override void OnPlayerDroppedItem(PlayerDroppedItemEventArgs ev)
    {
        LogEntry.Create($"Player {ev.Player.Nickname} has dropped a {ev.Pickup.Name}", ev.Player.UserId);
    }

    public override void OnPlayerCuffed(PlayerCuffedEventArgs ev)
    {
        LogEntry.Create($"Player {ev.Player.Nickname} has cuffed {ev.Target.Nickname}", ev.Player.UserId,
            ev.Target.UserId).CreateForRelated();
    }

    public override void OnPlayerUncuffed(PlayerUncuffedEventArgs ev)
    {
        LogEntry.Create($"Player {ev.Player.Nickname} has uncuffed {ev.Target.Nickname}", ev.Player.UserId,
            ev.Target.UserId).CreateForRelated();
    }

    private static string ItemsString(Player player)
    {
        Item[] items = player.Items.ToArray();
        
        return $"{items.Humanise(item => item.Name).OrIfEmpty("no")} {"item".MaybePluralise(items.Length)}";
    }

    public override void OnPlayerDying(PlayerDyingEventArgs ev)
    {
        if (ev.Attacker != null)
        {
            LogEntry.Create($"Player {ev.Player.Nickname} has been killed by {ev.Attacker.Nickname} with {ev.DamageHandler.Reason} in {ev.Player.CachedRoom.ShortName}. They had {ItemsString(ev.Player)}",
                ev.Player.UserId, ev.Attacker.UserId).CreateForRelated();
        }
        else
        {
            LogEntry.Create(
                $"Player {ev.Player.Nickname} has died in {ev.Player.CachedRoom.ShortName} due to {ev.DamageHandler.Reason}. They had {ItemsString(ev.Player)}",
                ev.Player.UserId);
        }
    }

    public override void OnPlayerUsedItem(PlayerUsedItemEventArgs ev)
    {
        LogEntry.Create($"Player {ev.Player.Nickname} has used {ev.UsableItem.Name}", ev.Player.UserId);
    }

    public override void OnPlayerEscaping(PlayerEscapingEventArgs ev)
    {
        if (ev.Player.IsDisarmed)
        {
            LogEntry.Create(
                $"Player {ev.Player.Nickname} has escaped, they were cuffed by {ev.Player.DisarmedBy!.Nickname}. They are now a {ev.NewRole.GetFullName()}",
                ev.Player.UserId, ev.Player.DisarmedBy.UserId).CreateForRelated();
        }
        else
        {
            LogEntry.Create(
                $"Player {ev.Player.Nickname} has escaped. They are now a {ev.NewRole.GetFullName()}",
                ev.Player.UserId);
        }
    }

    private static string RoomsString(IEnumerable<Room> rooms)
    {
        Room[] arr = rooms.ToArray();
        
        return $"{"room".MaybePluralise(arr.Length)} {arr.Humanise(room => room.ShortName).OrIfEmpty("none")}";
    }

    public override void OnPlayerInteractedDoor(PlayerInteractedDoorEventArgs ev)
    {
        LogEntry.Create(
            $"Player {ev.Player.Nickname} has interacted with a door in {RoomsString(ev.Door.Rooms)}",
            ev.Player.UserId);
    }

    public override void OnPlayerInteractedElevator(PlayerInteractedElevatorEventArgs ev)
    {
        LogEntry.Create(
            $"Player {ev.Player.Nickname} has interacted with an elevator in {RoomsString(ev.Elevator.Rooms)}",
            ev.Player.UserId);
    }

    public override void OnPlayerJoined(PlayerJoinedEventArgs ev)
    {
        LogEntry.Create($"Player {ev.Player.Nickname} has joined", ev.Player.UserId);
    }

    public override void OnPlayerLeft(PlayerLeftEventArgs ev)
    {
        LogEntry.Create($"Player {ev.Player.Nickname} has left, unfortunately their role is unknown...",
            ev.Player.UserId);
    }
}