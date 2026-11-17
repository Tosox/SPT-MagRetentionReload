using EFT;
using EFT.InventoryLogic;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Tosox.MagRetentionReload.State
{
    internal static class RetainedMagazines
    {
        internal static readonly ConditionalWeakTable<Player.FirearmController.ReloadExternalMagResult, List<IOperationResult>> Operations
            = new ConditionalWeakTable<Player.FirearmController.ReloadExternalMagResult, List<IOperationResult>>();
    }
}
