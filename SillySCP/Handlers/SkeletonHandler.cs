using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using PlayerRoles;
using SillySCP.API.Components;
using SillySCP.API.Interfaces;

namespace SillySCP.Handlers
{
    public class SkeletonHandler : IRegisterable
    {
        public void Init()
        {
            Exiled.Events.Handlers.Map.Generated += OnMapGenerated;
            Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;
        }

        public void Unregister()
        {
            Exiled.Events.Handlers.Map.Generated -= OnMapGenerated;
            Exiled.Events.Handlers.Player.ChangingRole -= OnChangingRole;
        }

        private void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if(ev.NewRole.IsScp() && ev.Player.IsScp) Log.Info(ev.Reason);
            if (ev.NewRole == RoleTypeId.Scp3114 && ev.Reason != SpawnReason.ForceClass) ev.IsAllowed = false;
        }
        
        private void OnMapGenerated()
        {
            List<Room> rooms = new()
            {
                Room.Get(RoomType.Hcz106),
                Room.Get(RoomType.HczCrossRoomWater),
                Room.Get(RoomType.HczTestRoom),
                Room.Get(RoomType.HczArmory)
            };

            foreach (PitKiller pit in rooms.Where(room => room != null).SelectMany(room => room.GameObject.GetComponentsInChildren<PitKiller>()))
            {
                pit.gameObject.layer = 14;
                pit.gameObject.AddComponent<CheckVoid>();
            }
        }
    }
}