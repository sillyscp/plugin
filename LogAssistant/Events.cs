using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Extensions;
using LabApi.Features.Wrappers;

namespace LogAssistant;

public class Events : CustomEventsHandler
{
    private static string GetRoomName(Room? room) => room?.GameObject.name.Replace("(Clone)", "") ?? "Unknown";
    private static string ListToCorrectEnglish<T>(IEnumerable<T> items, Func<T, string> itemGetter, string emptyResult = "")
    {
        IEnumerable<T> enumerable = items as T[] ?? items.ToArray();
        switch (enumerable.Count())
        {
            case 0:
                return emptyResult;
            case 1:
                return itemGetter(enumerable.ElementAt(0));
        }

        T? lastItem = enumerable.LastOrDefault();
        
        if (lastItem == null)
            return emptyResult;
        
        IEnumerable<T> itemsToJoin = enumerable.Take(enumerable.Count() - 1);

        return $"{string.Join(", ", itemsToJoin.Select(itemGetter))} and {itemGetter(lastItem)}";
    }
    
    public override void OnPlayerDroppedItem(PlayerDroppedItemEventArgs ev)
    {
        LogEntry.Create($"Player {ev.Player.Nickname} has dropped a {ev.Pickup.Type}", ev.Player.UserId);
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

    public override void OnPlayerDying(PlayerDyingEventArgs ev)
    {
        if (ev.Attacker != null)
        {
            LogEntry.Create($"Player {ev.Player.Nickname} has been killed by {ev.Attacker.Nickname} with {ev.DamageHandler.GetType().Name} in {GetRoomName(ev.Player.CachedRoom)}. They had {string.Join(", ", ev.Player.Items.Select(item => item.Type))} items",
                ev.Player.UserId, ev.Attacker.UserId).CreateForRelated();
        }
        else
        {
            LogEntry.Create(
                $"Player {ev.Player.Nickname} has died in {GetRoomName(ev.Player.CachedRoom)}. They had {string.Join(", ", ev.Player.Items.Select(item => item.Type))} items",
                ev.Player.UserId);
        }
    }

    public override void OnPlayerUsedItem(PlayerUsedItemEventArgs ev)
    {
        LogEntry.Create($"Player {ev.Player.Nickname} has used {ev.UsableItem.Type}", ev.Player.UserId);
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

    public override void OnPlayerInteractedDoor(PlayerInteractedDoorEventArgs ev)
    {
        LogEntry.Create(
            $"Player {ev.Player.Nickname} has interacted with a door in rooms {string.Join(", ", ev.Door.Rooms.Select(GetRoomName))}",
            ev.Player.UserId);
    }

    public override void OnPlayerInteractedElevator(PlayerInteractedElevatorEventArgs ev)
    {
        LogEntry.Create(
            $"Player {ev.Player.Nickname} has interacted with an elevator in rooms {string.Join(", ", ev.Elevator.Rooms.Select(GetRoomName))}",
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