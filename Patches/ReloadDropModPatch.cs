using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using Tosox.MagRetentionReload.State;

namespace Tosox.MagRetentionReload.Patches
{
    /// <summary>
    /// Stops EFT from also spawning the retained magazine on the ground.
    ///
    /// ReloadRunPatch clears vestTargetAddress so the engine takes its "drop the old magazine"
    /// path, then stows the magazine itself. The drop half still runs during the animation though:
    /// DropMod calls GameWorld.ThrowItem, which registers the magazine as world loot even though it
    /// is now sitting in the rig. That leaves the same magazine in the inventory AND in
    /// GameWorld.LootList, and at raid end BaseLocalGame.method_13 reports every insured item found
    /// in LootList as lost - so the magazine loses its insurance and the trader mails the
    /// "I'll look for your stuff" message despite the magazine never having been lost.
    /// </summary>
    public class ReloadDropModPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                typeof(Player.FirearmController.GClass2016),
                nameof(Player.FirearmController.GClass2016.DropMod)
            );
        }

        [PatchPrefix]
        public static bool Prefix(Player.FirearmController.GClass2016 __instance, Item droppedMod, EWeaponModType modType)
        {
            if (modType != EWeaponModType.mod_magazine || droppedMod == null)
                return true;

            var cmd = __instance.Gclass2006_0;
            if (cmd?.RemoveOldMagResult == null || cmd.RemoveOldMagResult.Item != droppedMod)
                return true;

            // Suppress the drop only when we actually stowed this magazine. Quick reloads are meant
            // to drop the magazine and ReloadRunPatch skips them, so they never get an entry here -
            // and neither does a retention whose Add failed. Suppressing either would detach the
            // magazine from the weapon without giving it anywhere to go, losing it entirely.
            //
            // The entry is guaranteed to still exist: DropMod runs from OnMagPuttedToRig, between
            // the reload command's RaiseEvents(Begin) and RaiseEvents(Succeed) that clears it.
            return !MagRetentionState.RetainedMagOps.TryGetValue(cmd, out _);
        }
    }
}
