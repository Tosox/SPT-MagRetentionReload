using EFT;
using EFT.InventoryLogic;

namespace Tosox.MagRetentionReload.RetainedReload
{
    public class RetainedReloadDescriptor : BaseDescriptorClass
    {
        public string CurrentMagazineId;

        public string ReplacementMagazineId;

        public GClass1950 WeaponSlotAddress;

        public GClass1950 RetainedAddress;

        public override GStruct152<BaseInventoryOperationClass> ToInventoryOperation(IPlayer player)
        {
            var currentMagazineResult = player.FindItemById(CurrentMagazineId, true, true);
            if (currentMagazineResult.Failed)
            {
                return currentMagazineResult.Error;
            }

            var replacementMagazineResult = player.FindItemById(ReplacementMagazineId, true, true);
            if (replacementMagazineResult.Failed)
            {
                return replacementMagazineResult.Error;
            }

            var weaponSlotAddressResult = Player.ToItemAddress(WeaponSlotAddress);
            if (weaponSlotAddressResult.Failed)
            {
                return weaponSlotAddressResult.Error;
            }

            var retainedAddressResult = Player.ToItemAddress(RetainedAddress);
            if (retainedAddressResult.Failed)
            {
                return retainedAddressResult.Error;
            }

            if (!(currentMagazineResult.Value is MagazineItemClass currentMagazine))
            {
                return new GClass1522("Current item is not a magazine");
            }

            if (!(replacementMagazineResult.Value is MagazineItemClass replacementMagazine))
            {
                return new GClass1522("Replacement item is not a magazine");
            }

            var retainedReloadResult = RetainedReloadFactory.Create(
                currentMagazine,
                replacementMagazine,
                weaponSlotAddressResult.Value,
                retainedAddressResult.Value,
                player.InventoryController,
                true
            );
            if (retainedReloadResult.Failed)
            {
                return retainedReloadResult.Error;
            }

            return new RetainedReloadOperation(OperationId, player.InventoryController, retainedReloadResult.Value);
        }

        public override string ToString()
        {
            return $"{base.ToString()}, CurrentMagazineId: {CurrentMagazineId}, ReplacementMagazineId: {ReplacementMagazineId}";
        }
    }
}
