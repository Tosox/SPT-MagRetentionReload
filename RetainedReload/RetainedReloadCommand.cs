using System.Collections.Generic;

namespace Tosox.MagRetentionReload.RetainedReload
{
    [System.Serializable]
    internal sealed class RetainedReloadCommand : GClass3473
    {
        public string Action = "RetainedReload";

        public string CurrentMagazine;

        public string ReplacementMagazine;

        public GClass3387.GClass3448 WeaponSlot;

        public GClass3387.GClass3448 RetainedSlot;

        public List<RetainedReloadItemSnapshot> RetainedItems;

        public override bool Queued => false;
    }
}
