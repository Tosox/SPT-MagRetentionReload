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
    public class ReloadUIContextPatch : ModulePatch
    {
        private static readonly FieldInfo fTraderController =
            AccessTools.Field(typeof(ItemUiContext), "traderControllerClass");

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                typeof(ItemUiContext),
                nameof(ItemUiContext.ReloadWeapon)
            );
        }

        /// <summary>
        /// Vanilla looks for a free spot for the old magazine across vest -> pockets -> backpack
        /// (GetPrioritizedGridsForUnloadedObject(backpackIncluded: true)) and swaps it there, or
        /// drops it when nothing is free. That search runs before the new magazine has moved out of
        /// its own cells, so the one spot guaranteed to be free - the cells the new magazine is
        /// vacating - is the one vanilla can never pick. Preferring it keeps the old magazine in the
        /// rig instead of the backpack, and works even when everything is full.
        ///
        /// Anything we cannot improve on falls through to vanilla, which keeps its own notifications
        /// and its own fallbacks.
        /// </summary>
        [PatchPrefix]
        public static bool Prefix(ItemUiContext __instance, Weapon weapon, IEnumerable<CompoundItem> collections)
        {
            if (weapon.IsUnderBarrelDeviceActive || __instance.method_16(weapon))
            {
                return true;
            }

            var traderController = (TraderControllerClass)fTraderController.GetValue(__instance);

            // Nothing to retain without a magazine already in the weapon
            var currentMagazine = weapon.GetCurrentMagazine();
            if (currentMagazine == null || !traderController.Examined(currentMagazine))
            {
                return true;
            }

            var magazineSlot = weapon.GetMagazineSlot();
            var foundMagazine = __instance.method_18(magazineSlot, collections);
            if (foundMagazine == null || foundMagazine.PinLockState == EItemPinLockState.Locked)
            {
                return true;
            }

            // Reload the weapon properly when it is in the player's hands so the animation plays
            // and the reload path's retention applies, rather than silently swapping magazines
            var handsController = GamePlayerOwner.MyPlayer?.HandsController as IFirearmHandsController;
            if (handsController != null && handsController.Item == weapon)
            {
                handsController.ReloadMag(foundMagazine, null, null);
                return false;
            }

            var retainedAddress = foundMagazine.CurrentAddress;
            if (retainedAddress == null)
            {
                return true;
            }

            // Swap resolves the circular dependency here - the old magazine wants the new one's
            // cells while the new one wants the weapon slot - and is the same vanilla operation
            // ReloadWeapon itself uses, so this costs exactly what vanilla costs.
            var swap = InteractionsHandlerClass.Swap(
                currentMagazine, retainedAddress, foundMagazine, magazineSlot.CreateItemAddress(), traderController, true);
            if (swap.Failed)
            {
                // The old magazine doesn't fit where the new one was (a drum for a 30-rounder, say),
                // so let vanilla run its vest -> pockets -> backpack -> drop chain instead
                return true;
            }

            // Same call vanilla makes for its own swap, so this costs exactly what vanilla costs.
            // The callback only has to surface a failure the simulation above didn't catch.
            traderController.TryRunNetworkTransaction(swap, new Callback(result =>
            {
                if (result.Failed)
                {
                    NotificationManagerClass.DisplayWarningNotification(result.Error, ENotificationDurationType.Default);
                }
            }));

            return false;
        }
    }
}
