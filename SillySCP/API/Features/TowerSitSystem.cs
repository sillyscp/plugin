using LabApi.Features.Wrappers;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079;
using SecretAPI.Extensions;
using UnityEngine;

namespace SillySCP.API.Features;

public sealed class SitInfo // if need be this could be renamed into PlayerState and used in other things (like a mid-game volunteer system)
{
    public readonly Player Player;
    public readonly PlayerRoleBase Role;

    public readonly List<Pickup> Inventory;
    public List<AmmoPickup> Ammo;

    public readonly float MaxHealth;
    public readonly float Health;
    
    public readonly float MaxHume;
    public readonly float Hume;

    public readonly Vector3 Position;

    public readonly int ComputerXp;
    public SitInfo(Player player)
    {
        Player = player;
        Role = player.RoleBase;
        
        Inventory = player.DropAllItems();
        Ammo = player.DropAllAmmo();
        
        // teleport items/ammo above the tutorial tower and lock the physics
        foreach (Pickup item in Inventory.Concat(Ammo))
        {
            item.IsLocked = true;
            item.IsInUse = true;
            RoleTypeId.Tutorial.GetRandomSpawnPosition(out Vector3 spawnPos, out _);
            item.Position = spawnPos + (Vector3.up * 25);
            item.PickupStandardPhysics!.Rb.detectCollisions = false;
            item.PickupStandardPhysics!.Rb.constraints = RigidbodyConstraints.FreezeAll;
            item.PickupStandardPhysics!.Rb.isKinematic = true;
            
            if (item is Scp018Projectile projectile) projectile.RemainingTime = 60 * 60; // if a round lasts more than an hour we have bigger problems qwq
        }
        MaxHealth = player.MaxHealth;
        Health = player.Health;
        
        MaxHume = player.MaxHumeShield;
        Hume = player.HumeShield;

        Position = player.Position;
        if (Player.RoleBase is Scp079Role computerRole)
        {
            computerRole.SubroutineModule.TryGetSubroutine(out Scp079TierManager tierManager);
            ComputerXp = tierManager.TotalExp ;
        }
    }

    public void RestorePlayer()
    {
        if (!Player.IsOnline) return; // just in case they disconnect and something tries to restore them 
        Player.SetRole(Role.RoleTypeId,reason:RoleChangeReason.RemoteAdmin, flags:RoleSpawnFlags.None);
        
        Player.MaxHealth = MaxHealth;
        Player.Health = Health;

        Player.MaxHumeShield = MaxHume;
        Player.HumeShield = Hume;
        
        Player.Position = Position;
        
        if (Player.RoleBase is Scp079Role computerRole)
        {
            computerRole.SubroutineModule.TryGetSubroutine(out Scp079TierManager tierManager);
            tierManager.TotalExp = ComputerXp;
        }

        foreach (Pickup item in Inventory)
        {
            Player.AddItem(item);
            item.Destroy();
        }

        foreach (AmmoPickup ammo in Ammo)
        {
            Player.AddAmmo(ammo.Type,ammo.Ammo);
            ammo.Destroy();
        }
        
    }
}


public static class TowerSitSystem
{
    public static Dictionary<Player,SitInfo> ActiveSits = new ();
    
    public static bool Start(Player player)
    {

        if (ActiveSits.ContainsKey(player)) return false;

        SitInfo sit = new(player);
        ActiveSits[player] = sit;
        
        player.SetRole(RoleTypeId.Tutorial);
        return true;
    }
    
    public static bool End(Player player)
    {
        if (!ActiveSits.TryGetValue(player, out SitInfo sit)) return false;
        sit.RestorePlayer();
        ActiveSits.Remove(player);
        return true;
    }
    
}