using System.Reflection.Emit;
using Exiled.API.Enums;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pools;
using HarmonyLib;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.Keycards.Snake;
using static HarmonyLib.AccessTools;
using MEC;
using Mirror;

namespace SillySCP.Patches
{
    [HarmonyPatch(typeof(ChaosKeycardItem), nameof(ChaosKeycardItem.ServerProcessCustomCmd))]
    public static class SnakePatch
    {
        // ReSharper disable once InconsistentNaming
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);
            int index = newInstructions.FindIndex(instruction => instruction.opcode == OpCodes.Ldc_I4_1);
            newInstructions.InsertRange(
                index - 1, 
                [
                    new (OpCodes.Ldarg_0),
                    new (OpCodes.Ldloc_1),
                    new (OpCodes.Call, Method(typeof(SnakePatch), nameof(OnCardUsed))),
                ]);
            
            foreach (CodeInstruction instruction in newInstructions)
                yield return instruction;
            
            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }

        private static void OnCardUsed(ChaosKeycardItem itemBase, SnakeNetworkMessage msg)
        {
            if (!msg.HasFlag(SnakeNetworkMessage.SyncFlags.GameOver)) return;
            Item item = Item.Get(itemBase);
            if (item == null) return;
            item.Owner.ShowHint("You failed...");
            Timing.CallDelayed(3, () =>
            {
                item.Owner.ExplodeEffect(ProjectileType.FragGrenade);
                item.Owner.Kill("You lost at snake... how could you...");
            });
        }
    }
}