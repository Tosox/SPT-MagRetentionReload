using Comfort.Common;
using EFT;
using EFT.Communications;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;

namespace Tosox.MagRetentionReload.Patches
{
    internal class ReloadUIContextPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                typeof(ItemUiContext),
                nameof(ItemUiContext.ReloadWeapon)
            );
        }

        [PatchPrefix]
        public static bool Prefix(
            ItemUiContext __instance,
            Weapon weapon,
            IEnumerable<CompoundItem> collections,
            TraderControllerClass ___traderControllerClass)
        {
            if (weapon.IsUnderBarrelDeviceActive || __instance.method_16(weapon))
                return true;

            // Nothing to retain without a magazine already in the weapon
            var currentMagazine = weapon.GetCurrentMagazine();
            if (currentMagazine == null || !___traderControllerClass.Examined(currentMagazine))
                return true;

            var magazineSlot = weapon.GetMagazineSlot();
            var foundMagazine = __instance.method_18(magazineSlot, collections);
            if (foundMagazine == null || foundMagazine.PinLockState == EItemPinLockState.Locked)
                return true;

            // Route a held weapon through the reload pipeline so it behaves exactly like pressing the reload key
            var handsController = GamePlayerOwner.MyPlayer?.HandsController as IFirearmHandsController;
            if (handsController != null && handsController.Item == weapon)
            {
                handsController.ReloadMag(foundMagazine, null, null);
                return false;
            }

            var retainedAddress = foundMagazine.CurrentAddress;
            if (retainedAddress == null)
                return true;

            var swap = InteractionsHandlerClass.Swap(
                currentMagazine, retainedAddress, foundMagazine, magazineSlot.CreateItemAddress(), ___traderControllerClass, true);
            if (swap.Failed)
                return true;

            ___traderControllerClass.TryRunNetworkTransaction(swap, new Callback(result =>
            {
                if (result.Failed)
                    NotificationManagerClass.DisplayWarningNotification(result.Error, ENotificationDurationType.Default);
            }));

            return false;
        }
    }
}
