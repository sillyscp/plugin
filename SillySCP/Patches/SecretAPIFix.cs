using HarmonyLib;
using LabApi.Features.Wrappers;
using SecretAPI.Features.UserSettings;
using UserSettings.ServerSpecific;

namespace SillySCP.Patches
{
    [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.SendToPlayer), new Type[] { typeof(ReferenceHub) })]
    public static class SecretAPIFix
    {
        private static bool Prefix(ReferenceHub hub)
        {
            CustomSetting.SendSettingsToPlayer(Player.Get(hub));
            return false;
        }
    }
}