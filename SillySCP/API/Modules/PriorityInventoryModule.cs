using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using MEC;
using UnityEngine;

namespace SillySCP.API.Modules
{
    public static class PriorityInventoryModule
    {
        public static void Main(Player player, List<ItemType> items)
        {
            var inventoryToSpawn = new List<ItemType>();
            var inventoryToDrop = new List<ItemType>();
            var oldItems = player.Items.Select(i => i.Type).ToList();
            player.ClearInventory();
            
            oldItems
                .Where(item => item.IsScp() || item == ItemType.KeycardO5 || item == ItemType.MicroHID ||
                               item == ItemType.GunFRMG0)
                .ToList().AddItems(inventoryToSpawn, inventoryToDrop);
            
            items.AddItems(inventoryToSpawn, inventoryToDrop);
            
            oldItems
                .Where(item => !inventoryToSpawn.Contains(item) && !inventoryToDrop.Contains(item))
                .ToList().AddItems(inventoryToSpawn, inventoryToDrop);
            
            items.Clear();
            items.AddRange(inventoryToSpawn);
            
            Timing.CallDelayed(1f, () =>
            {
                foreach (var item in inventoryToDrop)
                {
                    Pickup.CreateAndSpawn(item, player.Position, Quaternion.identity);
                }
            });
        }

        private static void AddItems(this List<ItemType> items, List<ItemType> inventoryToSpawn,
            List<ItemType> inventoryToDrop)
        {
            items.ForEach(item => item.AddItem(inventoryToSpawn, inventoryToDrop));
        }

        private static void AddItem(this ItemType item, List<ItemType> inventoryToSpawn, List<ItemType> inventoryToDrop)
        {
            if (inventoryToSpawn.Count >= 8)
            {
                inventoryToDrop.Add(item);
            }
            else
            {
                inventoryToSpawn.Add(item);
            }
        }
    }
}