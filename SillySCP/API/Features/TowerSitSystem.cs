using CustomPlayerEffects;
using InventorySystem.Items.Usables.Scp1344;
using LabApi.Features.Wrappers;
using NorthwoodLib.Pools;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079;
using SillySCP.API.Extensions;
using UnityEngine;
using Scp1344Item = LabApi.Features.Wrappers.Scp1344Item;

namespace SillySCP.API.Features;

public sealed class SitInfo // if need be this could be repurposed into a way to Stash a players state 
{
    public readonly Player Player;
    public readonly PlayerRoleBase Role;

    public readonly List<(StatusEffectBase Effect,float RemainingDuration,byte Intensity)> ActiveEffects = [];
    public readonly List<Pickup> Inventory;
    public readonly KeyValuePair<ItemType,ushort>[] Ammo;
    
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
        
        foreach (var effect in player.ActiveEffects) ActiveEffects.Add((effect,effect.TimeLeft,effect.Intensity));
        
        Inventory = player.CloneItems();
        Ammo = player.Ammo.ToArray();
        
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
        
        foreach (var effect in ActiveEffects) Player.EnableEffect(effect.Effect,effect.Intensity,effect.RemainingDuration);
        
        foreach (Pickup pickup in Inventory)
        {
            Item item = Player.AddItem(pickup);
            
            if (ActiveEffects.Exists(activeEffect => activeEffect.Effect is Scp1344))
                if (item is Scp1344Item scp1344) {
                    Player.DisableEffect<Scp1344>();
                    scp1344.Use(); //any effect that puts something on the players screen doesn't like being set frame 0
                    scp1344.Status = Scp1344Status.Active;
                }
        }
        ListPool<Pickup>.Shared.Return(Inventory);
        
        foreach (KeyValuePair<ItemType, ushort> ammo in Ammo) Player.AddAmmo(ammo.Key,ammo.Value);
        
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
    }
}

public static class TowerSitSystem
{
    public static Dictionary<Player,SitInfo> ActiveSits = new ();
    
    public static void Start(Player player)
    {
        if (ActiveSits.ContainsKey(player)) return;
        SitInfo sit = new(player);
        ActiveSits[player] = sit;
        
        player.SetRole(RoleTypeId.Tutorial);
    }
    
    public static void End(Player player)
    {
        if (!ActiveSits.TryGetValue(player, out SitInfo sit)) return;
        sit.RestorePlayer();
        ActiveSits.Remove(player);
    }
}