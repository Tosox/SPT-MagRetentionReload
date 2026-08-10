using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using Tosox.MagRetentionReload.State;

namespace Tosox.MagRetentionReload.Patches
{
    internal class ReloadDropModPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                typeof(Player.FirearmController.ReloadExternalMagOperation),
                nameof(Player.FirearmController.ReloadExternalMagOperation.DropMod)
            );
        }

        [PatchPrefix]
        public static bool Prefix(Player.FirearmController.ReloadExternalMagOperation __instance, Item droppedMod, EWeaponModType modType)
        {
            if (modType != EWeaponModType.mod_magazine || droppedMod == null)
                return true;

            var cmd = __instance.ReloadExternalMagResult;
            if (cmd?.RemoveOldMagResult == null || cmd.RemoveOldMagResult.Item != droppedMod)
                return true;

            // Stops EFT from also spawning the retained magazine on the ground
            return !MagRetentionState.RetainedMagOps.TryGetValue(cmd, out _);
        }
    }
}
