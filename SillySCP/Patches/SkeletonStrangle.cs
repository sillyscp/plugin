using CustomPlayerEffects;
using Exiled.API.Features;
using HarmonyLib;
using PlayerRoles.PlayableScps.Scp3114;
using PlayerRoles.Subroutines;
using RueI.Elements;
using SecretAPI.Features.UserSettings;
using SillySCP.API.Features;
using Utils.NonAllocLINQ;
using Player = LabApi.Features.Wrappers.Player;

namespace SillySCP.Patches
{
    [HarmonyPatch(typeof(SubroutineBase), nameof(SubroutineBase.ServerSendRpc), typeof(bool))]
    public static class SkeletonStrangle
    {
#pragma warning disable SA1313
        private static void Postfix(SubroutineBase __instance)
#pragma warning restore SA1313
        {
            if (__instance is not Scp3114Strangle strangle) return;
            if (strangle.SyncTarget.HasValue)
            {
                Log.Info("Target exists");
                Player player = Player.Get(strangle.SyncTarget.Value.Target);
                if (player == null) return;
                StruggleSetting setting = CustomSetting.GetPlayerSetting<StruggleSetting>(StruggleSetting.SettingId, player);
                if (setting == null) return;
                int elemCount = setting.Display?.Elements.Count ?? 0;
                if (elemCount == 1)
                {
                    Log.Info("Player has element already, so skipping");
                    return;
                }
                Log.Info("Adding hints");
                setting.Display ??= new(player.ReferenceHub);
                setting.Display.Elements.Add(StruggleSetting.Element);
                setting.Display.Update();
            }
            else
            {
                Log.Info("Target doesn't exist");
                Player player = Player.Dictionary.Values.FirstOrDefault(play =>
                {
                    if (play.HasEffect<Strangled>()) return false;
                    StruggleSetting setting = CustomSetting.GetPlayerSetting<StruggleSetting>(StruggleSetting.SettingId, play);
                    if (setting == null) return false;
                    return setting.Display?.Elements.Count == 1;
                });
                if (player == null) return;
                Log.Info("Player found, removing their hints.");
                StruggleSetting setting = CustomSetting.GetPlayerSetting<StruggleSetting>(StruggleSetting.SettingId, player)!;
                setting.Display!.Delete();
            }
        }
    }
}