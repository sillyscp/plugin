using LabApi.Features.Wrappers;
using MapGeneration;
using MEC;
using SecretAPI.Features;
using SillySCP.API.Components;
using SillySCP.API.Features;
using SillySCP.API.Extensions;

namespace SillySCP.Handlers
{
    public class MapHandler : IRegister
    {
        public void TryRegister()
        {
            RoomIdentifier.OnAdded += RoomAdded;
        }

        public void TryUnregister()
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
            
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (room.GameObject.GetStrippedName())
            {
                case "HCZ_Straight_PipeRoom":
                {
                    SpecialWeaponsPrimitive primitive = new (new (0, 7f, -4.8f), room, new (14f, 3, 3.9f));
                    primitive.Spawn();
                    break;
                }
                case "HCZ_127":
                {
                    // the spot with the ladder
                    SpecialWeaponsPrimitive ladderPrimitive = new(new(-5.75f, 0, 5.77f), room, new(3, 1, 7.5f), new(0, 45, 0));
                    ladderPrimitive.Spawn();
                
                    // the spot without the ladder
                    SpecialWeaponsPrimitive primitive = new(new (-5.34f, 0, -6.52f), room, new(5, 1, 3), new(0, 45, 0));
                    primitive.Spawn();
                    break;
                }
                case "Outside":
                {
                    SpecialWeaponsPrimitive primitive = new(new(21.60f, 1.5f, -22.74f), room, new(26, 1, 12));
                    primitive.Spawn();

                    SpecialWeaponsPrimitive pitPrimitive = new(new(0, -2, -7), room, new(23, 1, 12));
                    pitPrimitive.Spawn();
                    break;
                }
                case "HCZ_079":
                {
                    SpecialWeaponsPrimitive primitive = new(new(-5.40f, -3.5f, -13.5f), room, new(4.5f, 1, 6));
                    primitive.Spawn();
                    break;
                }
                case "HCZ_049":
                {
                    SpecialWeaponsPrimitive primitive = new(new(0, 0, -5f), room, new(15, 1, 5.2f));
                    primitive.Spawn();
                    break;
                }
            }
            
            foreach (PitKiller pit in room.GameObject.GetComponentsInChildren<PitKiller>())
            {
                pit.gameObject.layer = 14;
                pit.gameObject.AddComponent<CheckVoid>();
            }
        }
    }
}