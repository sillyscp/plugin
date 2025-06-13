using InventorySystem.Items.Pickups;
using JetBrains.Annotations;
using LabApi.Features.Wrappers;
using PlayerRoles;
using UnityEngine;

namespace SillySCP.API.Components
{
    public class CheckVoid : MonoBehaviour
    {
        public void OnTriggerEnter(Collider other)
        {
            // fuck you unity, unity only uses meshes instead of the actual obj for some reason
            Pickup pickup = Pickup.Get(other.gameObject.transform.root.gameObject.GetComponent<ItemPickupBase>());
            if (pickup?.LastOwner?.Role != RoleTypeId.Scp3114) return;
            Locker locker = pickup.Type switch
            {
                ItemType.MicroHID => GetLocker("MicroHIDpedestal"),
                ItemType.GunSCP127 => GetLocker("SCP_127_Container"),
                ItemType.ParticleDisruptor or ItemType.Jailbird => GetLocker("Experimental Weapon Locker"),
                _ => null
            };
            if(locker == null) return;
            Vector3 pos = locker.Position;
            pos.y += 1;
            pickup.Position = pos;
        }

        [CanBeNull]
        private Locker GetLocker(string lockerName)
        {
            return Locker.List.FirstOrDefault(locker => locker.Base.name.Split('(')[0].Trim() == lockerName);
        }
    }
}