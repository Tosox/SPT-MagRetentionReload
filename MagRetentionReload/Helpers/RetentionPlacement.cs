using Diz.LanguageExtensions;
using EFT.InventoryLogic;
using System.Collections.Generic;
using System.Linq;

namespace Tosox.MagRetentionReload.Helpers
{
    internal static class RetentionPlacement
    {
        // Addons may patch this. Add every inventory change to operations in order, and keep it
        // deterministic, since every Fika client runs it
        internal static bool Stow(
            Magazine oldMagazine, Magazine newMagazine, ItemAddress freedSlot, ItemController itemController,
            List<IOperationResult> operations)
        {
            foreach (var target in Targets(oldMagazine, newMagazine, freedSlot, itemController))
            {
                var attempt = ItemManipulator.Add(oldMagazine, target, itemController, false);
                if (attempt.Succeeded)
                {
                    operations.Add(attempt.Value);
                    return true;
                }
            }

            return false;
        }

        // The freed slot comes first unless the old magazine is smaller, so it doesn't break up that space.
        // No backpack or secure container, reaching them mid-reload shouldn't be free
        private static IEnumerable<ItemAddress> Targets(
            Magazine oldMagazine, Magazine newMagazine, ItemAddress freedSlot, ItemController itemController)
        {
            var fillsFreedSlot = !IsSmallerThan(oldMagazine, newMagazine);
            if (fillsFreedSlot)
                yield return freedSlot;

            var equipment = (itemController as InventoryController)?.Inventory?.Equipment;
            if (equipment != null)
            {
                foreach (var grid in equipment.GetPrioritizedGridsForUnloadedObject(false)
                    .OrderBy(grid => grid.GridWidth * grid.GridHeight))
                {
                    var freeSpace = grid.FindLocationForItem(oldMagazine);
                    if (freeSpace != null)
                        yield return freeSpace;
                }
            }

            if (!fillsFreedSlot)
                yield return freedSlot;
        }

        private static bool IsSmallerThan(Item item, Item other)
        {
            if (item == null || other == null)
                return false;

            var size = item.CalculateCellSize();
            var otherSize = other.CalculateCellSize();

            return size.X * size.Y < otherSize.X * otherSize.Y;
        }
    }
}
