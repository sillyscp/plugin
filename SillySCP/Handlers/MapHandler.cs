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
            if (room.Type == RoomType.Hcz127)
            {
                // the spot with the ladder
                SpecialWeaponsPrimitive ladderPrimitive = new(new(-5.75f, 0, 5.77f), room, new(3, 1, 7.5f), new(0, 45, 0));
                ladderPrimitive.Spawn();
                
                // the spot without the ladder
                SpecialWeaponsPrimitive primitive = new(new (-5.34f, 0, -6.52f), room, new(5, 1, 3), new(0, 45, 0));
                primitive.Spawn();
            }

            if (room.Type == RoomType.Surface)
            {
                SpecialWeaponsPrimitive primitive = new(new(21.60f, 1.5f, -22.74f), room, new(26, 1, 12));
                primitive.Spawn();

                SpecialWeaponsPrimitive pitPrimitive = new(new(0, -2, -7), room, new(23, 1, 12));
                pitPrimitive.Spawn();
            }
        }
    }
}