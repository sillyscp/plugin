using AdminToys;
using PrimitiveObjectToy = LabApi.Features.Wrappers.PrimitiveObjectToy;

namespace SillySCP.API.Extensions
{
    public static class PrimitiveExtensions
    {
        public static void SetVisibility(this PrimitiveObjectToy primitive, bool visibility) => primitive.Flags = visibility ? primitive.Flags | PrimitiveFlags.Visible : primitive.Flags & ~PrimitiveFlags.Visible;
        
        public static void SetCollidable(this PrimitiveObjectToy primitive, bool collidable) => primitive.Flags = collidable ? primitive.Flags | PrimitiveFlags.Collidable : primitive.Flags & ~PrimitiveFlags.Collidable;
    }
}