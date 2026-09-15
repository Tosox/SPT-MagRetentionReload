using EFT;
using EFT.InventoryLogic;
using System.Runtime.CompilerServices;

namespace Tosox.MagRetentionReload.State
{
    internal static class RetainedMagazines
    {
        /// <summary>
        /// The magazines this client is retaining, keyed by the reload they belong to
        /// </summary>
        internal static readonly ConditionalWeakTable<Player.FirearmController.ReloadExternalMagResult, AddResult> Operations
            = new ConditionalWeakTable<Player.FirearmController.ReloadExternalMagResult, AddResult>();
    }
}
