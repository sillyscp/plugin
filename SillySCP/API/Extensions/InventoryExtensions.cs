using InventorySystem.Items.Pickups;
using LabApi.Features.Wrappers;
using Mirror;
using PlayerRoles;
using SecretAPI.Extensions;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace SillySCP.API.Extensions;

public static class InventoryExtensions

{
    public static void FreezeItemPickup(ItemPickupBase pickup)
    {
        Logger.Info($"Freezing item pickup {pickup.name}");
        pickup.Info.Locked = true;
        pickup.Info.InUse = true;

        PickupStandardPhysics physics = pickup.PhysicsModule as PickupStandardPhysics;
        
        physics!.Rb.constraints = RigidbodyConstraints.FreezeAll;
        physics!.Rb.isKinematic = true;
        if (pickup is InventorySystem.Items.ThrowableProjectiles.Scp018Projectile projectile) projectile.TargetTime = NetworkTime.time + 60 * 60;
    }
    public static List<Pickup> CloneItems(this Player player)
    {
        List<Pickup> pickups = new ();
        foreach (var item in player.Items)
        {
            
            ItemPickupBase ipb = UnityEngine.Object.Instantiate(item.Base.PickupDropModel, Vector3.up, Quaternion.identity);
            ipb.NetworkInfo = new PickupSyncInfo(item.Base.ItemTypeId,item.Base.Weight,item.Base.ItemSerial,locked:true);
            FreezeItemPickup(ipb);
            Pickup pickup = Pickup.Get(ipb);
            
            pickups.Add(pickup);
        }
        return pickups;
    }
}