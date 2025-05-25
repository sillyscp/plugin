using Exiled.API.Features.Items;
using HarmonyLib;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.Keycards.Snake;
using MEC;
using Mirror;

namespace SillySCP.Patches
{
    [HarmonyPatch(typeof(ChaosKeycardItem), nameof(ChaosKeycardItem.ServerProcessCustomCmd))]
    public static class SnakePatch
    {
        // ReSharper disable once InconsistentNaming
        private static void Postfix(ChaosKeycardItem __instance, NetworkReader reader)
        {
            if ((ChaosKeycardItem.ChaosMsgType)reader.ReadByte() != ChaosKeycardItem.ChaosMsgType.SnakeMsgSync) return;
            SnakeNetworkMessage msg = new (reader);
            if (!msg.HasFlag(SnakeNetworkMessage.SyncFlags.GameOver)) return;
            Item item = Item.Get(__instance);
            if (item == null) return;
            item.Owner.ShowHint("You failed...");
            Timing.CallDelayed(3, () => item.Owner.Explode());
        }
    }
}