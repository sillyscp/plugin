using HarmonyLib;
using NorthwoodLib.Pools;

namespace SillySCP.Patches;
[HarmonyPatch(typeof(LabApi.Features.Wrappers.Player), nameof(LabApi.Features.Wrappers.Player.DropAllAmmo))]
public class DropAllAmmoPatch
{
    public static bool Prefix(LabApi.Features.Wrappers.Player __instance, ref List<LabApi.Features.Wrappers.AmmoPickup> __result)
    {
        List<LabApi.Features.Wrappers.AmmoPickup> ammo = ListPool<LabApi.Features.Wrappers.AmmoPickup>.Shared.Rent();
        foreach (KeyValuePair<ItemType, ushort> pair in __instance.Ammo.ToDictionary(e => e.Key, e => e.Value)) 
            ammo.AddRange(__instance.DropAmmo(pair.Key, pair.Value));
        
        __result = ammo;
        return false;
    }
}