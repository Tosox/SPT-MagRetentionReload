using Diz.LanguageExtensions;
using EFT.InventoryLogic;
using System.Collections.Generic;
using System.Linq;

namespace Tosox.MagRetentionReload.Helpers
{
    internal static class RetentionPlacement
    {
        /// <summary>
        /// Puts the old magazine away, in the first of its possible spots that accepts it, and reports
        /// the failure of the last one when none of them do so that the reload drops it like vanilla
        /// </summary>
        internal static OperationResult<AddResult> Stow(
            Magazine oldMagazine, Magazine newMagazine, ItemAddress freedSlot, ItemController itemController)
        {
            var attempt = default(OperationResult<AddResult>);

            foreach (var target in Targets(oldMagazine, newMagazine, freedSlot, itemController))
            {
                attempt = ItemManipulator.Add(oldMagazine, target, itemController, false);
                if (attempt.Succeeded)
                    return attempt;
            }

            return attempt;
        }

        /// <summary>
        /// Where the old magazine may go, best spot first. The slot the new magazine freed up comes
        /// first as long as the old one is at least as big, because then it fills that space instead
        /// of breaking it up. A smaller magazine goes wherever the game itself would put it and only
        /// takes the freed slot when nothing else has room, which still beats dropping it.
        ///
        /// The rig and the pockets are searched the way the game does it, smallest grid first. The
        /// backpack and the secure container are left out, as reaching either one mid-reload should
        /// not be free. Unlike the address the game hands to the reload, this runs once the new
        /// magazine has left the rig, so it also sees the space that the reload itself just freed up.
        /// </summary>
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
