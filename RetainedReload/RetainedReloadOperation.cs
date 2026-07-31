using Comfort.Common;
using EFT.InventoryLogic;
using System.Threading.Tasks;

namespace Tosox.MagRetentionReload.RetainedReload
{
    internal sealed class RetainedReloadOperation : GClass3475<RetainedReloadResult>, GInterface444
    {
        internal RetainedReloadOperation(ushort id, TraderControllerClass controller, RetainedReloadResult result)
            : base(id, controller, result)
        {
            CurrentMagazine = result.CurrentMagazine;
            CurrentFrom = result.WeaponSlotAddress;
            CurrentTo = result.RetainedAddress;
            ReplacementMagazine = result.ReplacementMagazine;
            ReplacementFrom = result.RetainedAddress;
            ReplacementTo = result.WeaponSlotAddress;
        }

        internal MagazineItemClass CurrentMagazine { get; }

        internal ItemAddress CurrentFrom { get; }

        internal ItemAddress CurrentTo { get; }

        internal MagazineItemClass ReplacementMagazine { get; }

        internal ItemAddress ReplacementFrom { get; }

        internal ItemAddress ReplacementTo { get; }

        public Item Item1 => ReplacementMagazine;

        public ItemAddress From1 => ReplacementFrom;

        public ItemAddress To1 => ReplacementTo;

        public Item Item2 => CurrentMagazine;

        public ItemAddress From2 => CurrentFrom;

        public ItemAddress To2 => CurrentTo;

        public override async Task<IResult> ExecuteInternal()
        {
            await method_3(CurrentMagazine, CurrentFrom, null);
            await method_3(ReplacementMagazine, ReplacementFrom, ReplacementTo);
            Execute();
            await method_4(ReplacementMagazine, ReplacementTo);
            await method_4(CurrentMagazine, CurrentTo);
            return method_5();
        }

        // Both wire formats below are deliberately vanilla. The end state of a retained reload -
        // the two magazines exchanging places - is exactly what a vanilla swap describes, so there
        // is no need for a custom action (and therefore no need for a server-side mod). Only the
        // local execution above stays custom, which is the whole point of this operation.
        public override BaseDescriptorClass ToDescriptor()
        {
            return new GClass1983
            {
                Operation = this,
                ItemId = CurrentMagazine.Id,
                To = GClass2061.FromItemAddress(CurrentTo),
                Item1Id = ReplacementMagazine.Id,
                To1 = GClass2061.FromItemAddress(ReplacementTo)
            };
        }

        public override GClass3471 ToBaseInventoryCommand(string ownerId)
        {
            return new SwapOperationClass.SwapCommand
            {
                Item = CurrentMagazine.Id,
                FromOwner = smethod_2(smethod_1(CurrentFrom, CurrentMagazine.OriginalAddress), ownerId),
                ToOwner = smethod_2(CurrentTo.GetOwner(), ownerId),
                To = smethod_0(CurrentTo),
                Item2 = ReplacementMagazine.Id,
                FromOwner2 = smethod_2(smethod_1(ReplacementFrom, CurrentMagazine.OriginalAddress), ownerId),
                ToOwner2 = smethod_2(ReplacementTo.GetOwner(), ownerId),
                To2 = smethod_0(ReplacementTo)
            };
        }

        public override string ToString()
        {
            return $"RetainedReload {CurrentMagazine.ToFullString()} -> {CurrentTo}, {ReplacementMagazine.ToFullString()} -> {ReplacementTo}";
        }
    }
}
