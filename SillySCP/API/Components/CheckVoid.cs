using Exiled.API.Enums;
using PlayerRoles;
using UnityEngine;
using Locker = Exiled.API.Features.Lockers.Locker;
using Pickup = Exiled.API.Features.Pickups.Pickup;

namespace SillySCP.API.Components
{
    public class CheckVoid : MonoBehaviour
    {
        public void OnTriggerEnter(Collider other)
        {
            // fuck you unity, unity only uses meshes instead of the actual obj for some reason
            Pickup pickup = Pickup.Get(other.gameObject.transform.root.gameObject);
            if (pickup.PreviousOwner.Role.Type != RoleTypeId.Scp3114) return;
            Locker locker = pickup.Type switch
            {
                ItemType.MicroHID => Locker.Get(LockerType.MicroHid).FirstOrDefault(),
                ItemType.GunSCP127 => Locker.Get(LockerType.Scp127Pedestal).FirstOrDefault(),
                ItemType.ParticleDisruptor or ItemType.Jailbird => Locker.Get(LockerType.ExperimentalWeapon).FirstOrDefault(),
                _ => null
            };
            if(locker == null) return;
            Vector3 pos = locker.Position;
            pos.y += 1;
            pickup.Position = pos;
        }
    }
}