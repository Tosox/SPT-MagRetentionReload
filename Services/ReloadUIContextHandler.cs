using Comfort.Common;
using EFT;
using EFT.Communications;
using EFT.InventoryLogic;
using EFT.UI;
using System.Collections.Generic;
using Tosox.MagRetentionReload.RetainedReload;

namespace Tosox.MagRetentionReload.Services
{
    internal static class ReloadUIContextHandler
    {
        internal static bool Handle(ItemUiContext uiContext, Weapon weapon, IEnumerable<CompoundItem> collections, TraderControllerClass traderController)
        {
            if (uiContext.method_16(weapon))
            {
                return false;
            }

            var magazineSlot = weapon.GetMagazineSlot();
            var foundMagazine = uiContext.method_18(magazineSlot, collections);
            if (foundMagazine == null)
            {
                ShowWarning("Can't find any non-empty magazine".Localized(null));
                return false;
            }

            if (foundMagazine.PinLockState == EItemPinLockState.Locked)
            {
                NotificationManagerClass.DisplaySingletonWarningNotification(
                    new InteractionsHandlerClass.GClass1606(foundMagazine).GetLocalizedDescription(),
                    ENotificationDurationType.Default
                );
                return false;
            }

            var currentMagazine = weapon.GetCurrentMagazine();
            if (currentMagazine != null && !traderController.Examined(currentMagazine))
            {
                NotificationManagerClass.DisplaySingletonWarningNotification(
                    "Attached magazine is not examined.".Localized(null),
                    ENotificationDurationType.Default
                );
                return false;
            }

            var magSlotAddress = magazineSlot.CreateItemAddress();
            var handsController = GamePlayerOwner.MyPlayer?.HandsController as IFirearmHandsController;
            if (currentMagazine != null && handsController != null && handsController.Item == weapon)
            {
                handsController.ReloadMag(foundMagazine, null, null);
                return false;
            }

            if (currentMagazine == null)
            {
                RunSimpleTransactionAsync(traderController, InteractionsHandlerClass.Move(foundMagazine, magSlotAddress, traderController, true));
                return false;
            }

            var retainedAddress = foundMagazine.CurrentAddress;
            if (retainedAddress == null)
            {
                ShowWarning("Can't find a place for item".Localized(null));
                return false;
            }

            if (Singleton<GameWorld>.Instance != null)
            {
                HandleInRaid(traderController, currentMagazine, foundMagazine, magSlotAddress, retainedAddress);
                return false;
            }

            RunSimpleTransactionAsync(
                traderController,
                RetainedReloadFactory.Create(currentMagazine, foundMagazine, magSlotAddress, retainedAddress, traderController, true)
            );
            return false;
        }

        /// <summary>
        /// Applies the retention immediately, then replicates it.
        ///
        /// Routing this through TryRunNetworkTransaction instead would cost a host round trip
        /// before anything moved: FIKA only executes a client's inventory operation locally once
        /// the host answers (ClientInventoryOperationHandler.ReceiveStatusFromServer). Applying it
        /// directly keeps it instant - the same approach the hotkey reload path uses - and the
        /// dispatched sync operation is a local no-op that only carries the change to the peers.
        /// </summary>
        private static void HandleInRaid(
            TraderControllerClass traderController,
            MagazineItemClass currentMagazine,
            MagazineItemClass foundMagazine,
            ItemAddress magSlotAddress,
            ItemAddress retainedAddress)
        {
            var applied = RetainedReloadFactory.Create(
                currentMagazine, foundMagazine, magSlotAddress, retainedAddress, traderController, false);
            if (applied.Failed)
            {
                ShowWarning(applied.Error.ToString());
                return;
            }

            // The Begin/Succeed pair matters: GClass3405.RaiseEvents marks the moved magazine as
            // known on Succeed, which is what keeps it from showing up as needing a search
            applied.Value.RaiseEvents(traderController, CommandStatus.Begin);
            applied.Value.RaiseEvents(traderController, CommandStatus.Succeed);

            var syncOperation = new RetainedReloadSyncOperation(
                traderController.method_12(), traderController,
                currentMagazine, foundMagazine, magSlotAddress, retainedAddress);

            // Never pass a null callback here - FIKA calls callback.Fail(..) unguarded when the
            // hands controller rejects an operation, which would throw instead of reporting
            traderController.vmethod_1(syncOperation, new Callback(OnSyncCompleted));
        }

        private static void OnSyncCompleted(IResult result)
        {
            // The retention itself already succeeded locally; this only reports replication issues
            if (result.Failed)
                ShowWarning(result.Error);
        }

        private static async void RunSimpleTransactionAsync(TraderControllerClass traderController, GStruct153 operationResult)
        {
            var result = await traderController.TryRunNetworkTransaction(operationResult, null);
            if (result.Failed)
            {
                ShowWarning(result.Error);
            }
        }

        private static void ShowWarning(string message)
        {
            NotificationManagerClass.DisplayWarningNotification(message, ENotificationDurationType.Default);
        }
    }
}
