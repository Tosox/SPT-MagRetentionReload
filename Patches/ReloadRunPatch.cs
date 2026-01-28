using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using Tosox.MagRetentionReload.State;

namespace Tosox.MagRetentionReload.Patches
{
    public class ReloadRunPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                typeof(Player.FirearmController.GClass2006),
                nameof(Player.FirearmController.GClass2006.Run)
            );
        }

        [PatchPrefix]
        public static void Prefix(
            Weapon weapon,
            MagazineItemClass nextMagazine,
            bool quickReload,
            ref ItemAddress vestTargetAddress,
            ref ItemAddress __state)
        {
            // Store original inventory location of the new mag
            __state = nextMagazine?.Parent;

            // Force the EFT mag "drop" logic
            if (!quickReload && weapon?.GetCurrentMagazine() != null)
                vestTargetAddress = null;
        }

        [PatchPostfix]
        public static void Postfix(
            TraderControllerClass itemController,
            bool quickReload,
            ItemAddress __state,
            GStruct156<Player.FirearmController.GClass2006> __result)
        {
            if (__result.Failed || quickReload)
                return;

            // Skip if there was no mag in the weapon
            var cmd = __result.Value;
            if (cmd.RemoveOldMagResult == null || __state == null)
                return;

            // Avoid double processing just in case
            if (MagRetentionState.RetainedMagazine.TryGetValue(cmd, out _))
                return;

            // The old mag EFT just removed from the weapon
            var oldMag = cmd.RemoveOldMagResult.Item as MagazineItemClass;
            if (oldMag == null)
                return;

            // Insert old mag into the slot that was freed by reloading the weapon
            var addOldMag = InteractionsHandlerClass.Add(oldMag, __state, itemController, false);
            if (addOldMag.Failed)
                return;

            // Attach the Add-result to this reload command
            MagRetentionState.RetainedMagazine.Add(cmd, addOldMag.Value);
        }
    }
}
