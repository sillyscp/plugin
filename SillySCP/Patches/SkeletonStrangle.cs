using Exiled.API.Features;
using HarmonyLib;
using PlayerRoles.PlayableScps.Scp3114;
using SillySCP.API.Features;
using Player = LabApi.Features.Wrappers.Player;

namespace SillySCP.Patches
{
    [HarmonyPatch(typeof(Scp3114Strangle), nameof(Scp3114Strangle.SyncTarget), MethodType.Setter)]
    public static class SkeletonStrangle
    {
#pragma warning disable SA1313
        private static bool Prefix(Scp3114Strangle __instance, Scp3114Strangle.StrangleTarget? value)
#pragma warning restore SA1313
        {
            if (!value.HasValue && !__instance.SyncTarget.HasValue) return true;
            Log.Info(value.HasValue);
            if(!value.HasValue) {
                Player player = Player.Get(__instance.SyncTarget.Value.Target);
                StruggleSetting.Remove(player);
            } else {
                Player player = Player.Get(value.Value.Target);
                StruggleSetting.Add(player);
            }

            return true;
        }
    }
}