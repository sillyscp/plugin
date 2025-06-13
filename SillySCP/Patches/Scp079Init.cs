using HarmonyLib;
using PlayerRoles.PlayableScps.Scp079;
using SillySCP.API.Modules;

namespace SillySCP.Patches
{
    [HarmonyPatch(typeof(Scp079Recontainer), nameof(Scp079Recontainer.Start))]
    public static class Scp079Init
    {
        public static void Postfix(Scp079Recontainer __instance)
        {
            Scp079Recontainment.Recontainer = __instance;
        }
    }
}