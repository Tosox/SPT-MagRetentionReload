using Comfort.Common;
using EFT.InventoryLogic;

namespace Tosox.MagRetentionReload.RetainedReload
{
    /// <summary>
    /// Replicates an already-applied retained reload to the other peers.
    ///
    /// In a raid the retention is performed directly (and instantly) against the local inventory,
    /// because FIKA gates a client's inventory operations on a host round trip before executing
    /// them locally. This operation therefore does nothing locally - it only exists to carry the
    /// change onto the wire.
    ///
    /// Both wire formats it produces are deliberately vanilla types: FIKA serialises a descriptor
    /// by looking its type up in a hardcoded table (GClass3695.WritePolymorph), so a mod-defined
    /// descriptor would serialise to zero bytes, the host would never confirm the operation, and
    /// the pending operation would wedge the hands controller. GClass1983 rebuilds a real vanilla
    /// SwapOperationClass on the receiving peer, and SwapCommand is the native "Swap" action.
    /// </summary>
    internal sealed class RetainedReloadSyncOperation : BaseInventoryOperationClass
    {
        internal RetainedReloadSyncOperation(
            ushort id,
            TraderControllerClass controller,
            MagazineItemClass currentMagazine,
            MagazineItemClass replacementMagazine,
            ItemAddress weaponSlotAddress,
            ItemAddress retainedAddress)
            : base(id, controller)
        {
            CurrentMagazine = currentMagazine;
            ReplacementMagazine = replacementMagazine;
            WeaponSlotAddress = weaponSlotAddress;
            RetainedAddress = retainedAddress;
        }

        internal MagazineItemClass CurrentMagazine { get; }

        internal MagazineItemClass ReplacementMagazine { get; }

        internal ItemAddress WeaponSlotAddress { get; }

        internal ItemAddress RetainedAddress { get; }

        public override void ExecuteInternal(Callback callback)
        {
            // The local inventory was already changed before this operation was dispatched
            callback?.Invoke(SuccessfulResult.New);
        }

        public override BaseDescriptorClass ToDescriptor()
        {
            // Mirrors SwapOperationClass.ToDescriptor: the old magazine takes the new magazine's
            // place, the new magazine goes into the weapon. Built from the addresses captured
            // before the local mutation, since the items have already moved by now.
            return new GClass1983
            {
                Operation = this,
                ItemId = CurrentMagazine.Id,
                To = GClass2061.FromItemAddress(RetainedAddress),
                Item1Id = ReplacementMagazine.Id,
                To1 = GClass2061.FromItemAddress(WeaponSlotAddress)
            };
        }

        public override GClass3471 ToBaseInventoryCommand(string ownerId)
        {
            return new SwapOperationClass.SwapCommand
            {
                Item = CurrentMagazine.Id,
                FromOwner = smethod_2(smethod_1(WeaponSlotAddress, CurrentMagazine.OriginalAddress), ownerId),
                ToOwner = smethod_2(RetainedAddress.GetOwner(), ownerId),
                To = smethod_0(RetainedAddress),
                Item2 = ReplacementMagazine.Id,
                FromOwner2 = smethod_2(smethod_1(RetainedAddress, CurrentMagazine.OriginalAddress), ownerId),
                ToOwner2 = smethod_2(WeaponSlotAddress.GetOwner(), ownerId),
                To2 = smethod_0(WeaponSlotAddress)
            };
        }

        public override string ToString()
        {
            return $"RetainedReloadSync {CurrentMagazine.Id} -> {RetainedAddress}, {ReplacementMagazine.Id} -> {WeaponSlotAddress}";
        }

        public override void Dispose()
        {
        }
    }
}
