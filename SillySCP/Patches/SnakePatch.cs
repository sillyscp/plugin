using System.Reflection.Emit;
using HarmonyLib;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.Keycards.Snake;
using LabApi.Features.Wrappers;
using static HarmonyLib.AccessTools;
using MEC;
using NorthwoodLib.Pools;
using Utils;

namespace SillySCP.Patches
{
    [HarmonyPatch(typeof(ChaosKeycardItem), nameof(ChaosKeycardItem.ServerProcessCustomCmd))]
    public static class SnakePatch
    {
        // ReSharper disable once InconsistentNaming
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);
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
            
            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }

        private static void OnCardUsed(ChaosKeycardItem itemBase, SnakeNetworkMessage msg)
        {
            if (!msg.HasFlag(SnakeNetworkMessage.SyncFlags.GameOver)) return;
            Item item = Item.Get(itemBase);
            if (item == null) return;
            item.CurrentOwner!.SendHint("You failed...");
            Timing.CallDelayed(3, () =>
            {
                ExplosionUtils.ServerSpawnEffect(item.CurrentOwner!.Position, ItemType.GrenadeHE);
                item.CurrentOwner!.Kill("You lost at snake... how could you...");
            });
        }
    }
}