using HarmonyLib;
using PlayerRoles.PlayableScps.Scp3114;
using SillySCP.API.Features;

namespace SillySCP.Patches
{
    [HarmonyPatch(typeof(Scp3114Strangle), nameof(Scp3114Strangle.ServerProcessCmd))]
    public static class StrangleStart
    {
        public static bool Prefix(Scp3114Strangle __instance)
        {
            SkeletonDataStore store = SkeletonDataStore.GetFromStrangle(__instance);
            if (store == null) return true;
            if (!store.CanStartStrangle) return false;
            store.Cooldown.Trigger(10);
            return true;
        }
    }
}