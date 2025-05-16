using Exiled.API.Enums;
using Exiled.API.Features;
using MapGeneration;
using MEC;
using SillySCP.API.Features;
using SillySCP.API.Interfaces;
using UnityEngine;

namespace SillySCP.Handlers
{
    public class MapHandler : IRegisterable
    {
        public void Init()
        {
            RoomIdentifier.OnAdded += RoomAdded;
        }

        public void Unregister()
        {
            RoomIdentifier.OnAdded -= RoomAdded;
        }

        private static void RoomAdded(RoomIdentifier r)
        {
            // me when exiled doesn't get the rooms here, so have to do a small delay :3
            Timing.CallDelayed(0, () => OnRoomAdded(r));
        }

        private static void OnRoomAdded(RoomIdentifier r)
        {
            Room room = Room.Get(r);
            if (r.name.ToLower().Contains("HCZ_Straight_PipeRoom".ToLower()))
            {
                SpecialWeaponsPrimitive primitive = new (new (0, 7f, -4.8f), room, new (14f, 3, 3.9f));
                primitive.Spawn();
            }
        }
    }
}