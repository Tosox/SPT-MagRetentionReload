using EFT;
using System.Runtime.CompilerServices;

namespace Tosox.MagRetentionReload.State
{
    internal static class MagRetentionState
    {
        internal static readonly ConditionalWeakTable<Player.FirearmController.GClass2006, GClass3405> RetainedMagazine
            = new ConditionalWeakTable<Player.FirearmController.GClass2006, GClass3405>();
    }
}
