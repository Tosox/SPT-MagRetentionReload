using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
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
            if (__result.Failed || quickReload || __state == null)
                return;

            // Skip if there was no mag in the weapon
            var cmd = __result.Value;
            if (cmd.RemoveOldMagResult == null)
                return;

            // Avoid double processing just in case
            if (MagRetentionState.RetainedMagOps.TryGetValue(cmd, out _))
                return;

            // The old mag EFT just removed from the weapon
            if (!(cmd.RemoveOldMagResult.Item is MagazineItemClass oldMag))
                return;

            // Leave AI alone
            var owner = (itemController as Player.PlayerInventoryController)?.Player_0;
            if (owner == null || IsAiPlayer(owner))
                return;

            // Insert old mag into the slot that was freed by reloading the weapon
            var addOldMagOp = InteractionsHandlerClass.Add(oldMag, __state, itemController, false);
            if (addOldMagOp.Failed)
                return;

            // Attach the Add-operation result to this reload command
            MagRetentionState.RetainedMagOps.Add(cmd, addOldMagOp.Value);
        }

        private static readonly Dictionary<Type, FieldInfo> ObservedAiFields = new Dictionary<Type, FieldInfo>();

        private static bool IsAiPlayer(Player player)
        {
            if (player.IsAI)
                return true;

            var type = player.GetType();
            if (!ObservedAiFields.TryGetValue(type, out var field))
            {
                field = type.GetField("IsObservedAI", BindingFlags.Public | BindingFlags.Instance);
                ObservedAiFields[type] = field;
            }

            return field != null && (bool)field.GetValue(player);
        }
    }
}
