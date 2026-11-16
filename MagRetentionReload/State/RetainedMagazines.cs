using EFT;
using EFT.InventoryLogic;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Tosox.MagRetentionReload.State
{
    internal static class RetainedMagazines
    {
        /// <summary>
        /// What this client did to retain a magazine, keyed by the reload it belongs to, in the order it
        /// happened. Usually just the Add, but an addon may have moved other items out of the way first
        /// </summary>
        internal static readonly ConditionalWeakTable<Player.FirearmController.ReloadExternalMagResult, List<IOperationResult>> Operations
            = new ConditionalWeakTable<Player.FirearmController.ReloadExternalMagResult, List<IOperationResult>>();
    }
}
