using CustomPlayerEffects;
using HarmonyLib;
using PlayerRoles.PlayableScps.Scp3114;
using SecretAPI.Features.UserSettings;
using SillySCP.API.Features;
using Utils.NonAllocLINQ;
using Player = LabApi.Features.Wrappers.Player;

namespace SillySCP.Patches
{
    [HarmonyPatch(typeof(Scp3114Strangle), nameof(Scp3114Strangle._rpcType), MethodType.Setter)]
    public static class SkeletonStrangle
    {
#pragma warning disable SA1313
        private static bool Prefix()
#pragma warning restore SA1313
        {
            foreach (KeyValuePair<Player, List<CustomSetting>> pair in CustomSetting.PlayerSettings)
            {
                if(!pair.Value.TryGetFirst(sett => sett.GetType() == typeof(StruggleSetting), out CustomSetting setting)) continue;
                if(setting is not StruggleSetting struggle) continue;
                if(!pair.Key.HasEffect<Strangled>() && (struggle.Display?.Elements.Contains(StruggleSetting.Element) ?? false))
                {
                    struggle.Display.Delete();
                    continue;
                }
                if (!pair.Key.HasEffect<Strangled>() || (struggle.Display?.Elements.Contains(StruggleSetting.Element) ?? false)) continue;
                struggle.Percentage = 0;
                struggle.Display ??= new(pair.Key.ReferenceHub);
                struggle.Display.Elements.Add(StruggleSetting.Element);
                struggle.Display.Update();
            }
            return true;
        }
    }
}