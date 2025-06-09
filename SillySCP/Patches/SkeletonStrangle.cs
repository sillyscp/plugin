using HarmonyLib;
using LabApi.Features.Wrappers;
using PlayerRoles.PlayableScps.Scp3114;
using SecretAPI.Features.UserSettings;
using SillySCP.API.Settings;
using Player = LabApi.Features.Wrappers.Player;

namespace SillySCP.Patches
{
    [HarmonyPatch(typeof(Scp3114Strangle), nameof(Scp3114Strangle.ServerWriteRpc))]
    public static class SkeletonStrangle
    {
#pragma warning disable SA1313
        private static void Postfix(Scp3114Strangle __instance)
#pragma warning restore SA1313
        {
            if (__instance.SyncTarget.HasValue)
            {
                Player player = Player.Get(__instance.SyncTarget.Value.Target);
                if (player == null) return;
                StruggleSetting setting = CustomSetting.GetPlayerSetting<StruggleSetting>(StruggleSetting.SettingId, player);
                if (setting == null) return;
                int elemCount = setting.Display?.Elements.Count ?? 0;
                if (elemCount == 1)
                {
                    return;
                }

                if (player.CurrentItem is UsableItem { IsUsing: true })
                {
                    ItemType itemType = player.CurrentItem.Type;
                    player.RemoveItem(player.CurrentItem);
                    player.AddItem(itemType);
                }
                setting.Percentage = 0;
                setting.Display ??= new(player.ReferenceHub);
                setting.Display.Elements.Add(StruggleSetting.Element);
                setting.Display.Update();
            }
            else
            {
                StruggleSetting.RemoveFromFirst();
            }
        }
    }
}