using Exiled.API.Features;
using Exiled.API.Features.Toys;
using SillySCP.API.Components;
using UnityEngine;

namespace SillySCP.API.Features
{
    public class SpecialWeaponsPrimitive
    {
        public SpecialWeaponsPrimitive(Vector3 localPos, Room room, Vector3 size)
        {
            LocalPosition = localPos;
            Room = room;
            Size = size;
        }

        private Vector3 LocalPosition { get; set; }
        
        private Vector3 Size { get; set; }
        
        private Room Room { get; set; }

        public void Spawn()
        {
            Primitive primitive = Primitive.Create(PrimitiveType.Cube, Room.WorldPosition(LocalPosition), Room.Rotation.eulerAngles, Size);
            primitive.Collidable = false;
            primitive.GameObject.AddComponent<CheckVoid>();
            BoxCollider collider = primitive.GameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
        }
    }
}