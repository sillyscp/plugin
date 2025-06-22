using System.Reflection.Emit;
using HarmonyLib;
using PlayerRoles.PlayableScps.Scp3114;

namespace SillySCP.Patches
{
    [HarmonyPatch(typeof(Scp3114Slap), nameof(Scp3114Slap.DamagePlayers))]
    public static class SkeletonSlapHS
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchEndForward(new CodeMatch(OpCodes.Ldc_R4, 25f))
                .SetOperandAndAdvance(10f);

            return matcher.InstructionEnumeration();
        }
    }
}