using Diz.LanguageExtensions;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
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
            if (quickReload || weapon?.GetCurrentMagazine() == null)
                return;

            var owner = itemController.GetPlayer();
            if (owner == null || owner.IsAi())
                return;

            if (!RetentionRules.IsRetaining(owner, weapon, nextMagazine))
                return;

            var sourceAddress = nextMagazine?.Parent;
            if (sourceAddress == null)
                return;

            // No target makes vanilla drop the old magazine, which the postfix stows instead
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
            if (__state == null || __result.Failed)
                return;

            var cmd = __result.Value;
            if (cmd.RemoveOldMagResult == null)
                return;

            if (RetainedMagazines.Operations.TryGetValue(cmd, out _))
                return;

            if (!(cmd.RemoveOldMagResult.Item is Magazine oldMag))
                return;

            var operations = new List<IOperationResult>();
            if (!RetentionPlacement.Stow(oldMag, nextMagazine, __state, itemController, operations))
                return;

            RetainedMagazines.Operations.Add(cmd, operations);

            var owner = itemController.GetPlayer();
            if (owner != null && owner.IsYourPlayer)
                RetentionSync.RetentionAnnouncer?.Invoke(owner.ProfileId, nextMagazine?.Id);
        }
    }
}
