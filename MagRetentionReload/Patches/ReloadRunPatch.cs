using Diz.LanguageExtensions;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using Tosox.MagRetentionReload.Helpers;
using Tosox.MagRetentionReload.State;
using Tosox.MagRetentionReload.Sync;

namespace Tosox.MagRetentionReload.Patches
{
    internal class ReloadRunPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                typeof(Player.FirearmController.ReloadExternalMagResult),
                nameof(Player.FirearmController.ReloadExternalMagResult.Run)
            );
        }

        [PatchPrefix]
        public static void Prefix(
            ItemController itemController,
            Weapon weapon,
            Magazine nextMagazine,
            bool quickReload,
            ref ItemAddress vestTargetAddress,
            ref ItemAddress __state)
        {
            // Skip if there is no mag in the weapon to retain
            if (quickReload || weapon?.GetCurrentMagazine() == null)
                return;

            // Leave AI alone
            var owner = itemController.GetPlayer();
            if (owner == null || owner.IsAi())
                return;

            if (!RetentionRules.IsRetaining(owner, weapon, nextMagazine))
                return;

            // Original inventory location of the new mag, freed up by the reload
            var sourceAddress = nextMagazine?.Parent;
            if (sourceAddress == null)
                return;

            // Only force the EFT mag "drop" logic once we know where to put the old mag
            __state = sourceAddress;
            vestTargetAddress = null;
        }

        [PatchPostfix]
        public static void Postfix(
            ItemController itemController,
            Magazine nextMagazine,
            ItemAddress __state,
            Option<Player.FirearmController.ReloadExternalMagResult> __result)
        {
            // The prefix only sets a state when it forced the drop logic
            if (__state == null || __result.Failed)
                return;

            var cmd = __result.Value;
            if (cmd.RemoveOldMagResult == null)
                return;

            // Avoid double processing just in case
            if (RetainedMagazines.Operations.TryGetValue(cmd, out _))
                return;

            // The old mag EFT just removed from the weapon
            if (!(cmd.RemoveOldMagResult.Item is Magazine oldMag))
                return;

            // Insert old mag into the slot that was freed by reloading the weapon
            var addOldMagOp = ItemManipulator.Add(oldMag, __state, itemController, false);
            if (addOldMagOp.Failed)
                return;

            // Attach the Add-operation result to this reload command
            RetainedMagazines.Operations.Add(cmd, addOldMagOp.Value);

            // Tell the other clients to retain this reload as well
            var owner = itemController.GetPlayer();
            if (owner != null && owner.IsYourPlayer)
                RetentionSync.RetentionAnnouncer?.Invoke(owner.ProfileId, nextMagazine?.Id);
        }
    }
}
