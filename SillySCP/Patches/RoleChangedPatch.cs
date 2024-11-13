using HarmonyLib;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Armor;
using InventorySystem.Items.Pickups;
using Mirror;
using PlayerRoles;

namespace SillySCP.Patches;

public class RoleChangedPatch
{
    [HarmonyPatch(typeof(InventoryItemProvider), nameof(PlayerEffectsController.OnRoleChanged))]
    private static void RoleChanged(ReferenceHub ply, PlayerRoleBase prevRole, PlayerRoleBase newRole)
    {
        if (!NetworkServer.active || !newRole.ServerSpawnFlags.HasFlag(RoleSpawnFlags.AssignInventory))
        {
            return;
        }

        var inventory = ply.inventory;
        var flag = InventoryItemProvider.KeepItemsAfterEscaping && newRole.ServerSpawnReason == RoleChangeReason.Escaped;
        if (flag)
        {
            var list = new List<ItemPickupBase>();
            if (inventory.TryGetBodyArmor(out var bodyArmor))
            {
                bodyArmor.DontRemoveExcessOnDrop = true;
            }
            
            var items = new Dictionary<ushort, ItemBase>();
            var oldItems = new Dictionary<ushort, ItemBase>(inventory.UserInventory.Items);
            
            foreach (var item in inventory.UserInventory.Items)
            {
                switch (item.Value.ItemTypeId)
                {
                    case ItemType.KeycardO5:
                    {
                        items.AddItem(item);
                        oldItems.Remove(item.Key);
                        break;
                    }
                    case ItemType.SCP018:
                    {
                        items.AddItem(item);
                        oldItems.Remove(item.Key);
                        break;
                    }
                    case ItemType.SCP2176:
                    {
                        items.AddItem(item);
                        oldItems.Remove(item.Key);
                        break;
                    }
                    case ItemType.SCP268:
                    {
                        items.AddItem(item);
                        oldItems.Remove(item.Key);
                        break;
                    }
                    case ItemType.GunFRMG0:
                    {
                        items.AddItem(item);
                        oldItems.Remove(item.Key);
                        break;
                    }
                }
            }
            
            inventory.UserInventory.Items.Clear();

            foreach (var item in oldItems)
            {
                inventory.UserInventory.Items.Add(item.Key, item.Value);
            }

            foreach (var item in items)
            {
                inventory.UserInventory.Items.Add(item.Key, item.Value);
            }
            
            while (inventory.UserInventory.Items.Count > 0)
            {
                list.Add(inventory.ServerDropItem(inventory.UserInventory.Items.ElementAt(0).Key));
            }

            InventoryItemProvider.PreviousInventoryPickups[ply] = list;
        }
        InventoryItemProvider.ServerGrantLoadout(ply, newRole.RoleTypeId, !flag);
        InventoryItemProvider.InventoriesToReplenish.Enqueue(ply);
    }
}