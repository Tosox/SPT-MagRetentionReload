using System.Collections.Generic;
using EFT.InventoryLogic;

namespace Tosox.MagRetentionReload.RetainedReload
{
    public sealed class RetainedReloadItemSnapshot
    {
        public string ItemId;

        public string TemplateId;

        public int StackCount;

        public bool SpawnedInSession;

        public GClass3387.GClass3448 Address;
    }

    internal static class RetainedReloadSnapshotBuilder
    {
        internal static List<RetainedReloadItemSnapshot> Build(Item rootItem)
        {
            var snapshots = new List<RetainedReloadItemSnapshot>();
            if (rootItem == null)
            {
                return snapshots;
            }

            foreach (var item in rootItem.GetAllVisibleItems())
            {
                var address = item.CurrentAddress;
                if (address == null)
                {
                    continue;
                }

                snapshots.Add(new RetainedReloadItemSnapshot
                {
                    ItemId = item.Id,
                    TemplateId = item.StringTemplateId,
                    StackCount = item.StackObjectsCount,
                    SpawnedInSession = item.SpawnedInSession,
                    Address = BaseInventoryOperationClass.smethod_0(address)
                });
            }

            return snapshots;
        }
    }
}
