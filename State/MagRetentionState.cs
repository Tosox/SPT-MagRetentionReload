using EFT;
using EFT.InventoryLogic;
using System.Runtime.CompilerServices;

namespace Tosox.MagRetentionReload.State
{
    internal static class MagRetentionState
    {
        internal static readonly ConditionalWeakTable<Player.FirearmController.ReloadExternalMagResult, AddResult> RetainedMagOps
            = new ConditionalWeakTable<Player.FirearmController.ReloadExternalMagResult, AddResult>();
    }
}
