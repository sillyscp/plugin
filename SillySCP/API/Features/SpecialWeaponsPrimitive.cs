using AdminToys;
using LabApi.Features.Wrappers;
using SillySCP.API.Components;
using SillySCP.API.Extensions;
using UnityEngine;
using PrimitiveObjectToy = LabApi.Features.Wrappers.PrimitiveObjectToy;

namespace SillySCP.API.Features
{
    public class SpecialWeaponsPrimitive
    {
        public SpecialWeaponsPrimitive(Vector3 localPos, Room room, Vector3? size = null, Vector3? rotationOffset = null)
        {
            LocalPosition = localPos;
            Room = room;
            Size = size ?? Vector3.one;
            RotationOffset = rotationOffset ?? Vector3.zero;
        }

        private Vector3 LocalPosition { get; set; }
        
        private Vector3 Size { get; set; }
        
        private Room Room { get; set; }
        
        private Vector3 RotationOffset { get; set; }

        public void Spawn()
        {
            PrimitiveObjectToy primitive = PrimitiveObjectToy.Create(Room.Transform.TransformPoint(LocalPosition), Quaternion.Euler(Room.Rotation.eulerAngles + RotationOffset), Size, networkSpawn:false);
            primitive.Type = PrimitiveType.Cube;
            primitive.Spawn();
            primitive.SetVisibility(false);
            primitive.SetCollidable(false);
            primitive.GameObject.AddComponent<CheckVoid>();
            BoxCollider collider = primitive.GameObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
        }
    }
}