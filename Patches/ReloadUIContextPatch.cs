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
            ItemController ____itemController)
        {
            if (weapon.IsUnderBarrelDeviceActive || __instance.TryExamineMalfunction(weapon))
                return true;

            // Nothing to retain without a magazine already in the weapon
            var currentMagazine = weapon.GetCurrentMagazine();
            if (currentMagazine == null || !____itemController.Examined(currentMagazine))
                return true;

            var magazineSlot = weapon.GetMagazineSlot();
            var foundMagazine = __instance.FindSuitableMagazine(magazineSlot, collections);
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

            var swap = ItemManipulator.Swap(
                currentMagazine, retainedAddress, foundMagazine, magazineSlot.CreateItemAddress(), ____itemController, true);
            if (swap.Failed)
                return true;

            ____itemController.TryRunNetworkTransaction(swap, new Callback(result =>
            {
                if (result.Failed)
                    NotificationManager.DisplayWarningNotification(result.Error, ENotificationDurationType.Default);
            }));

            return false;
        }
    }
}
