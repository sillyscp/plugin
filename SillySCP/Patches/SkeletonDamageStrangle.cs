using HarmonyLib;
using PlayerRoles.PlayableScps.Scp3114;
using SillySCP.API.Settings;

namespace SillySCP.Patches
{
    [HarmonyPatch(typeof(Scp3114Strangle), nameof(Scp3114Strangle.OnThisPlayerDamaged))]
    public static class SkeletonDamageStrangle
    {
        private static void Postfix()
        {
            StruggleSetting.RemoveFromFirst();
        }
    }
}