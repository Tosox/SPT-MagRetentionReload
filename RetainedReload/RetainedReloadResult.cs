using EFT.InventoryLogic;

namespace Tosox.MagRetentionReload.RetainedReload
{
    internal sealed class RetainedReloadResult : GInterface433, GInterface424
    {
        internal RetainedReloadResult(
            MagazineItemClass currentMagazine,
            ItemAddress weaponSlotAddress,
            MagazineItemClass replacementMagazine,
            ItemAddress retainedAddress,
            TraderControllerClass itemController,
            GClass3410 removeResult,
            GClass3411 loadResult,
            GClass3405 retainResult)
        {
            CurrentMagazine = currentMagazine;
            WeaponSlotAddress = weaponSlotAddress;
            ReplacementMagazine = replacementMagazine;
            RetainedAddress = retainedAddress;
            TraderControllerClass = itemController;
            RemoveResult = removeResult;
            LoadResult = loadResult;
            RetainResult = retainResult;
        }

        internal MagazineItemClass CurrentMagazine { get; }

        internal ItemAddress WeaponSlotAddress { get; }

        internal MagazineItemClass ReplacementMagazine { get; }

        internal ItemAddress RetainedAddress { get; }

        internal TraderControllerClass TraderControllerClass { get; }

        internal GClass3410 RemoveResult { get; }

        internal GClass3411 LoadResult { get; }

        internal GClass3405 RetainResult { get; }

        public Item Item => CurrentMagazine;

        public Item ResultItem => ReplacementMagazine;

        public ItemAddress From => WeaponSlotAddress;

        public bool CanExecute(TraderControllerClass itemController)
        {
            return CurrentMagazine != null
                && ReplacementMagazine != null
                && WeaponSlotAddress != null
                && RetainedAddress != null;
        }

        public GStruct153 Execute()
        {
            return RetainedReloadFactory.Create(
                CurrentMagazine,
                ReplacementMagazine,
                WeaponSlotAddress,
                RetainedAddress,
                TraderControllerClass,
                false
            );
        }

        public void RaiseEvents(IItemOwner controller, CommandStatus status)
        {
            RemoveResult.RaiseEvents(controller, status);
            LoadResult.RaiseEvents(controller, status);
            RetainResult.RaiseEvents(controller, status);
        }

        public void RollBack()
        {
            RetainResult.RollBack();
            LoadResult.RollBack();
            RemoveResult.RollBack();
        }
    }
}
