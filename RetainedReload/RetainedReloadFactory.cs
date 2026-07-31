using EFT.InventoryLogic;

namespace Tosox.MagRetentionReload.RetainedReload
{
    internal static class RetainedReloadFactory
    {
        internal static GStruct154<RetainedReloadResult> Create(
            MagazineItemClass currentMagazine,
            MagazineItemClass replacementMagazine,
            ItemAddress weaponSlotAddress,
            ItemAddress retainedAddress,
            TraderControllerClass itemController,
            bool simulate)
        {
            if (currentMagazine == null)
            {
                return new GClass1522("Current magazine is null");
            }

            if (replacementMagazine == null)
            {
                return new GClass1522("Replacement magazine is null");
            }

            if (weaponSlotAddress == null)
            {
                return new GClass1522("Weapon slot address is null");
            }

            if (retainedAddress == null)
            {
                return new GClass1522("Retained address is null");
            }

            if (currentMagazine.Id == replacementMagazine.Id)
            {
                return new GClass1522("Cannot reload with the same magazine");
            }

            var removeResult = InteractionsHandlerClass.Remove(currentMagazine, itemController, false);
            if (removeResult.Failed)
            {
                return removeResult.Error;
            }

            var loadResult = InteractionsHandlerClass.Move(replacementMagazine, weaponSlotAddress, itemController, false);
            if (loadResult.Failed)
            {
                removeResult.Value.RollBack();
                return loadResult.Error;
            }

            var retainResult = InteractionsHandlerClass.Add(currentMagazine, retainedAddress, itemController, false);
            if (retainResult.Failed)
            {
                loadResult.Value.RollBack();
                removeResult.Value.RollBack();
                return retainResult.Error;
            }

            if (simulate)
            {
                retainResult.Value.RollBack();
                loadResult.Value.RollBack();
                removeResult.Value.RollBack();
            }

            return new RetainedReloadResult(
                currentMagazine,
                weaponSlotAddress,
                replacementMagazine,
                retainedAddress,
                itemController,
                removeResult.Value,
                loadResult.Value,
                retainResult.Value
            );
        }
    }
}
