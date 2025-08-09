using HarmonyLib;
using InventorySystem.Items.ThrowableProjectiles;
using LabApi.Features.Wrappers;

namespace SillySCP.Patches
{
    [HarmonyPatch(typeof(FlashbangGrenade), nameof(FlashbangGrenade.ProcessPlayer))]
    public static class FlashbangPatch
    {
        public static bool Prefix(ReferenceHub hub)
        {
            Player player = Player.Get(hub);
            if (player == null) return true;
            return !player.IsDisarmed;
        }
    }
}