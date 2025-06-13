using System.Reflection.Emit;
using HarmonyLib;
using InventorySystem.Items.Jailbird;
using LabApi.Features.Wrappers;
using Mirror;
using SillySCP.Handlers;
using JailbirdItem = InventorySystem.Items.Jailbird.JailbirdItem;

namespace SillySCP.Patches
{
    [HarmonyPatch(nameof(JailbirdItem), nameof(JailbirdItem.ServerProcessCmd))]
    public static class JailbirdPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchStartForward(
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(NetworkReader), nameof(NetworkReader.ReadByte)))
                )
                .Advance(2)
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(JailbirdPatch), nameof(OnEvent)))
                );

            return matcher.InstructionEnumeration();
        }

        private static void OnEvent(JailbirdItem instance, JailbirdMessageType type)
        {
            if (type != JailbirdMessageType.AttackTriggered) return;
            Item item = Item.Get(instance);
            SSSSHandler.OnJailbirdEvent(item.CurrentOwner!);
        }
    }
}