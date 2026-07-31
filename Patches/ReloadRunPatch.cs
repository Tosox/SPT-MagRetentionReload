using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Linq;
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

            // Leave AI alone - their gear is host-authoritative and retaining for them would
            // diverge from what other peers see on their corpses. Anything we cannot positively
            // identify as AI is treated as a player, so this never silently disables retention.
            if (IsAiOwner(itemController))
                return;

            // Insert old mag into the slot that was freed by reloading the weapon.
            // This runs on every peer: FIKA mirrors each player's reload by replaying this same
            // method locally, so all clients reach the same result without any packets of ours.
            // Sending our own network transaction here would double-apply the move and wedge the
            // hands controller, since FIKA holds inventory operations until the host confirms.
            var addOldMagOp = InteractionsHandlerClass.Add(oldMag, __state, itemController, false);
            if (addOldMagOp.Failed)
                return;

            // Attach the Add-operation result to this reload command
            MagRetentionState.RetainedMagOps.Add(cmd, addOldMagOp.Value);
        }

        private static bool IsAiOwner(TraderControllerClass itemController)
        {
            var owner = Singleton<GameWorld>.Instance?.AllAlivePlayersList
                .FirstOrDefault(p => p.InventoryController == itemController);

            return owner != null && owner.IsAI;
        }
    }
}
