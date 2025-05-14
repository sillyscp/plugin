using HarmonyLib;
using Hints;
using RueI;
using SillySCP.API.Features;

namespace SillySCP.Patches
{
    [HarmonyPatch(typeof(UnityProvider), "ShowHint")]
    public static class RueISendHint
    {
        public static bool Prefix(ref ReferenceHub hub, ref string message)
        {
            if (!message.Contains(StruggleSetting.Hint)) return true;
            hub.connectionToClient.Send(new HintMessage(new TextHint(message, new HintParameter[] { new SSKeybindHintParameter(StruggleSetting.SettingId) }, null, 99999)));
            return false;
        }
    }
}